using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class T_IPV6ADDR : pointer
	{
		public struct Fields
		{
			public static readonly array_field_info<byte> s6_addr8 = new array_field_info<byte>(0, 16);
			public static readonly array_field_info<ushort> s6_addr16 = new array_field_info<ushort>(0, 8, true);
			public static readonly array_field_info<uint> s6_addr32 = new array_field_info<uint>(0, 4, true);
		}
		public new static int length = 16;

		public T_IPV6ADDR(byte[] data, int offset) : base(data, offset) { }
		public T_IPV6ADDR(pointer data, int offset) : base(data, offset) { }
		public T_IPV6ADDR(pointer data) : base(data) { }

		public array<byte> s6_addr8 { get { return GetArrayField(Fields.s6_addr8); } }
		public array<ushort> s6_addr16 { get { return GetArrayField(Fields.s6_addr16); } }
		public array<uint> s6_addr32 { get { return GetArrayField(Fields.s6_addr32); } }
	}

	public class T_IPV6EP : pointer
	{
		public struct Fields
		{
			public static readonly array_field_info<byte> ipaddr = new array_field_info<byte>(0, 16);
			public static readonly value_field_info<ushort> portno = new value_field_info<ushort>(16, true);
		}
		public new static int length = 18;

		public T_IPV6EP(byte[] data, int offset) : base(data, offset) { }
		public T_IPV6EP(pointer data, int offset) : base(data, offset) { }
		public T_IPV6EP(pointer data) : base(data) { }

		public array<byte> ipaddr { get { return GetArrayField(Fields.ipaddr); } }
		public ushort portno { get { return GetFieldValue(Fields.portno); } set { SetFieldValue(Fields.portno, value); } }
	}

	public struct T_UDP6_CCEP
	{
		public ATR cepatr;
		public T_IPV6EP myaddr;
		public CepCallback callback;
		public object exinf;
	}

	public struct T_UDP6_RCEP
	{
		public object exinf;
		public bool wtsk;
		public pointer pk_dat;
	}

	internal class Udp6Cep : StateMachine<bool>
	{
		Nucleus m_Nucleus;
		ID m_CepID;
		T_UDP6_CCEP m_cudp;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		LinkedList<pointer> m_DatQueue = new LinkedList<pointer>();
		Queue<pointer> m_CallBack = new Queue<pointer>();
		lwip m_lwIP;
		udp_pcb m_Pcb;

		public Udp6Cep(ID udpid, ref T_UDP6_CCEP pk_cudp, Nucleus pNucleus, lwip lwip)
		{
			m_CepID = udpid;
			m_cudp = pk_cudp;
			m_Nucleus = pNucleus;
			m_lwIP = lwip;

			ip6_addr addr = new ip6_addr(pk_cudp.myaddr.ipaddr);
			m_Pcb = m_lwIP.udp.udp_new();
			//m_lwIP.udp.udp_bind(m_Pcb, addr, pk_cudp.myaddr.portno);
			//udp.udp_recv(m_Pcb, recv, this);
		}

		public ID CepID { get { return m_CepID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_UDP6_CCEP cudp { get { return m_cudp; } }

		public ER ReferStatus(ref T_UDP6_RCEP pk_rudp)
		{
			//if (pk_rudp == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rudp.exinf = m_cudp.exinf;

			// 待ちタスクの有無
			pk_rudp.wtsk = m_TskQueue.First != null;

			// 待ちメッセージの有無
			pk_rudp.pk_dat = new pointer((m_DatQueue.First == null) ? null : m_DatQueue.First.Value, 0);

			return ER.E_OK;
		}

		public ER SendData(T_IPV6EP p_dstaddr, pointer p_dat, int len, TMO tmout)
		{
			pbuf buf;
			ip6_addr addr = new ip6_addr(p_dstaddr.ipaddr);

			buf = m_lwIP.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT, (ushort)len, pbuf_type.PBUF_POOL);
			if (buf == null)
				return ER.E_NOMEM;

			int pos = 0;
			for (pbuf q = buf; q != null; q = q.next) {
				pointer.memcpy(q.payload, new pointer(p_dat, pos), q.len);
				pos += q.len;
			}

			//m_lwIP.udp.udp_sendto(m_Pcb, buf, addr, p_dstaddr.portno);

			return ER.E_OK;
		}

		private void recv(object arg, udp_pcb pcb, pbuf p, ip6_addr addr, ushort port)
		{
			System.Diagnostics.Debug.Assert((arg == this) && (pcb == m_Pcb));
			pointer data = new pointer(new byte[sizeof(ushort) + T_IPV6EP.length + p.tot_len], 0);
			T_IPV6EP p_dstaddr = new T_IPV6EP(data, sizeof(ushort));
			pointer p_dat = new pointer(data, sizeof(ushort) + T_IPV6EP.length);

			data.SetValue(p.tot_len);
			pointer.memcpy(p_dstaddr.ipaddr, addr, T_IPV6ADDR.length);
			p_dstaddr.portno = lwip.lwip_ntohs(port);

			for (pbuf q = p; q != null; q = q.next) {
				pointer.memcpy(p_dat, q.payload, q.len);
				p_dat += q.len;
			}

			lock (m_CallBack) {
				m_CallBack.Enqueue(data);
			}

			SetState(true, TMO.TMO_POL, null, OnTimeOut);
		}

		private void OnTimeOut(object arg)
		{
			pointer data;

			lock (m_CallBack) {
				if (m_CallBack.Count == 0)
					return;

				data = m_CallBack.Dequeue();
			}

			int len = (ushort)data;

			lock (m_DatQueue) {
				m_DatQueue.AddLast(data);
			}

			pointer parblk = new pointer(BitConverter.GetBytes(len), 0);

			m_cudp.callback(m_CepID, FN.TEV_UDP_RCV_DAT, parblk);
		}

		public ER ReceiveData(T_IPV6EP p_dstaddr, pointer p_dat, int len, TMO tmout)
		{
			ER ret;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			if ((p_dat == null) || (len < 0))
				return ER.E_PAR;

			if ((m_TskQueue.First == null) && (m_DatQueue.First != null)) {
				pointer data;
				lock (m_DatQueue) {
					data = m_DatQueue.First.Value;
					m_DatQueue.RemoveFirst();
				}

				int dlen = (ushort)data;
				pointer.memcpy(p_dstaddr, data + sizeof(ushort), T_IPV6EP.length);
				pointer.memcpy(p_dat, data + sizeof(ushort) + T_IPV6EP.length, dlen);

				return (ER)dlen;
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_UDP, m_CepID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_DatQueue.First == null)
						return ER.E_RLWAI;
					lock (m_DatQueue) {
						p_dat = new pointer(m_DatQueue.First.Value, 0);
						m_DatQueue.RemoveFirst();
					}
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			//SetDebugInfo(p_dat, lpszFileName, nLine);

			return ER.E_OK;
		}
	}
}
