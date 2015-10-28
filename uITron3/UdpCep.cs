using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class T_IPV4EP : pointer
	{
		public struct Fields
		{
			public static readonly value_field_info<uint> ipaddr = new value_field_info<uint>(0, true);
			public static readonly value_field_info<ushort> portno = new value_field_info<ushort>(4, true);
		}
		public new static int length = 6;

		public T_IPV4EP(byte[] data, int offset) : base(data, offset) { }
		public T_IPV4EP(pointer data, int offset) : base(data, offset) { }
		public T_IPV4EP(pointer data) : base(data) { }

		public uint ipaddr { get { return GetFieldValue(Fields.ipaddr); } set { SetFieldValue(Fields.ipaddr, value); } }
		public ushort portno { get { return GetFieldValue(Fields.portno); } set { SetFieldValue(Fields.portno, value); } }
	}

	/// <summary>
	/// API 機能・事象コード
	/// </summary>
	public enum FN
	{
		/* TCP 関係 */
		TFN_TCP_CRE_REP = -0x201,
		TFN_TCP_DEL_REP = -0x202,
		TFN_TCP_CRE_CEP = -0x203,
		TFN_TCP_DEL_CEP = -0x204,
		TFN_TCP_ACP_CEP = -0x205,
		TFN_TCP_CON_CEP = -0x206,
		TFN_TCP_SHT_CEP = -0x207,
		TFN_TCP_CLS_CEP = -0x208,
		TFN_TCP_SND_DAT = -0x209,
		TFN_TCP_RCV_DAT = -0x20a,
		TFN_TCP_GET_BUF = -0x20b,
		TFN_TCP_SND_BUF = -0x20c,
		TFN_TCP_RCV_BUF = -0x20d,
		TFN_TCP_REL_BUF = -0x20e,
		TFN_TCP_SND_OOB = -0x20f,
		TFN_TCP_RCV_OOB = -0x210,
		TFN_TCP_CAN_CEP = -0x211,
		TFN_TCP_SET_OPT = -0x212,
		TFN_TCP_GET_OPT = -0x213,
		TFN_TCP_ALL = 0,

		TEV_TCP_RCV_OOB = -0x201,

		/* UDP 関係 */

		TFN_UDP_CRE_CEP = -0x221,
		TFN_UDP_DEL_CEP = -0x222,
		TFN_UDP_SND_DAT = -0x223,
		TFN_UDP_RCV_DAT = -0x224,
		TFN_UDP_CAN_CEP = -0x225,
		TFN_UDP_SET_OPT = -0x226,
		TFN_UDP_GET_OPT = -0x227,
		TFN_UDP_ALL = 0,

		TEV_UDP_RCV_DAT = -0x221,
	}

	public delegate void CepCallback(ID cepid, FN fncd, pointer p_parblk);

	public struct T_UDP_CCEP
	{
		public ATR cepatr;
		public T_IPV4EP myaddr;
		public CepCallback callback;
		public object exinf;
	}

	public struct T_UDP_RCEP
	{
		public object exinf;
		public bool wtsk;
		public pointer pk_dat;
	}

	internal class UdpCep : StateMachine<bool>
	{
		Nucleus m_Nucleus;
		ID m_CepID;
		T_UDP_CCEP m_cudp;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		LinkedList<pointer> m_DatQueue = new LinkedList<pointer>();
		Queue<pointer> m_CallBack = new Queue<pointer>();
		lwip m_lwIP;
		udp_pcb m_Pcb;

		public UdpCep(ID udpid, ref T_UDP_CCEP pk_cudp, Nucleus pNucleus, lwip lwip)
		{
			m_CepID = udpid;
			m_cudp = pk_cudp;
			m_Nucleus = pNucleus;
			m_lwIP = lwip;

			ip_addr addr = new ip_addr(pk_cudp.myaddr.ipaddr);
			m_Pcb = m_lwIP.udp.udp_new();
			m_lwIP.udp.udp_bind(m_Pcb, addr, pk_cudp.myaddr.portno);
			udp.udp_recv(m_Pcb, recv, this);
		}

		public ID CepID { get { return m_CepID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_UDP_CCEP cudp { get { return m_cudp; } }

		public ER ReferStatus(ref T_UDP_RCEP pk_rudp)
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

		public ER SendData(T_IPV4EP p_dstaddr, pointer p_dat, int len, TMO tmout)
		{
			pbuf buf;
			ip_addr addr = new ip_addr(p_dstaddr.ipaddr);

			buf = m_lwIP.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT, (ushort)len, pbuf_type.PBUF_POOL);
			if (buf == null)
				return ER.E_NOMEM;

			for (pbuf q = buf; q != null; q = q.next) {
				pointer.memcpy(p_dat, q.payload, q.len);
			}

			m_lwIP.udp.udp_sendto(m_Pcb, buf, addr, p_dstaddr.portno);

			return ER.E_OK;
		}

		private void recv(object arg, udp_pcb pcb, pbuf p, ip_addr addr, ushort port)
		{
			System.Diagnostics.Debug.Assert((arg == this) && (pcb == m_Pcb));
			pointer data = new pointer(new byte[sizeof(ushort) + T_IPV4EP.length + p.tot_len], 0);
			T_IPV4EP p_dstaddr = new T_IPV4EP(data, sizeof(ushort));
			pointer p_dat = new pointer(data, sizeof(ushort) + T_IPV4EP.length);

			data.SetValue(p.tot_len);
			p_dstaddr.ipaddr = lwip.lwip_ntohl(addr.addr);
			p_dstaddr.portno = lwip.lwip_ntohs(port);

			for (pbuf q = p; q != null; q = q.next) {
				pointer.memcpy(p_dat, q.payload, q.len);
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
				data = m_CallBack.Dequeue();
			}

			int len = (ushort)data;

			lock (m_DatQueue) {
				m_DatQueue.AddLast(data);
			}

			pointer parblk = new pointer(BitConverter.GetBytes(len), 0);

			m_cudp.callback(m_CepID, FN.TEV_UDP_RCV_DAT, parblk);
		}

		public ER ReceiveData(T_IPV4EP p_dstaddr, pointer p_dat, int len, TMO tmout)
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
				pointer.memcpy(p_dstaddr, data + sizeof(ushort), T_IPV4EP.length);
				pointer.memcpy(p_dat, data + sizeof(ushort) + T_IPV4EP.length, dlen);

				return (ER)dlen;
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_TskQueue, (TSKWAIT)0x80, m_CepID, tmout);

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
