using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct T_TCP_CCEP
	{
		public const ushort TCP_PORTANY = 0;

		public ATR cepatr;
		public CepCallback callback;
		public object exinf;
	}

	public struct T_TCP_CREP
	{
		public ATR repatr;              /* 受付口属性		*/
		public T_IPV4EP myaddr;         /* 自分のアドレス	*/
		public object exinf;
	}

	internal class TcpCep : StateMachine<bool>
	{
		enum BufferMode
		{
			None,
			NormalCopy,
			ReduceCopy,
		};

		Nucleus m_Nucleus;
		ID m_CepID;
		T_TCP_CCEP m_ctcp;
		lwip m_lwIP;
		tcp_pcb m_Pcb;
		bool m_Shutdown;
		tcp_pcb m_TPcb;
		BufferMode m_SendMode;
		BufferMode m_RecvMode;
		LinkedList<Task> m_SendTaskQueue = new LinkedList<Task>();
		LinkedList<Task> m_RecvTaskQueue = new LinkedList<Task>();

		pointer m_SendPBuf;
		int m_SendPos;
		int m_SendLen;
		Queue<pbuf> m_RecvDataQueue = new Queue<pbuf>();
		pbuf m_RecvPBuf;
		pbuf m_RecvPBufPos;
		int m_RecvPos;

		byte[] m_RCSendData = new byte[1024];
		int m_RCSendWrPos;
		int m_RCSendRdPos;
		int m_RCSendSdPos;

		public TcpCep(ID tcpid, ref T_TCP_CCEP pk_ctcp, Nucleus pNucleus, lwip lwip)
		{
			m_CepID = tcpid;
			m_ctcp = pk_ctcp;
			m_Nucleus = pNucleus;
			m_lwIP = lwip;

			m_Pcb = m_lwIP.tcp.tcp_new();
			tcp.tcp_arg(m_Pcb, this);
			tcp.tcp_err(m_Pcb, Error);
		}

		public ID CepID { get { return m_CepID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_TCP_CCEP ctcp { get { return m_ctcp; } }

		public ER Accept(TcpRep tcpRep, T_IPV4EP p_dstaddr, TMO tmout)
		{
			tcp_pcb_listen pcb = m_lwIP.tcp.tcp_listen(m_Pcb);

			return tcpRep.Accept(pcb, this, p_dstaddr, tmout);
		}

		public ER Connect(T_IPV4EP p_myaddr, T_IPV4EP p_dstaddr, TMO tmout)
		{
			err_t err;
			ip_addr ma = new ip_addr(0);
			ip_addr sa = new ip_addr(0);

			ma.addr = lwip.lwip_htonl(p_myaddr.ipaddr);
			err = m_lwIP.tcp.tcp_bind(m_Pcb, ma, lwip.lwip_htons(p_myaddr.portno));
			if (err != err_t.ERR_OK)
				return ER.E_OBJ;

			sa.addr = lwip.lwip_htonl(p_dstaddr.ipaddr);
			err = m_lwIP.tcp.tcp_connect(m_Pcb, sa, lwip.lwip_htons(p_dstaddr.portno), Connected);
			if (err != err_t.ERR_OK)
				return ER.E_OBJ;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			if (task == null)
				return ER.E_TMOUT;

			if (tmout == 0)
				return ER.E_TMOUT;

			SetState(true, new TMO(tcp.TCP_TMR_INTERVAL), task, Intarval);

			ER ret = task.Wait(m_SendTaskQueue, TSKWAIT.TTW_TCP, m_CepID, tmout);

			switch (ret) {
			case ER.E_OK:
				if ((m_TPcb == null) || (m_TPcb.state != tcp_state.ESTABLISHED))
					return ER.E_RLWAI;
				break;
			case ER.E_TMOUT:
				return ER.E_TMOUT;
			default:
				return ER.E_RLWAI;
			}

			return ER.E_OK;
		}

		private void Intarval(object data)
		{
			if ((m_Pcb == null) || (m_Pcb.state == tcp_state.CLOSED))
				SetState(false);

			m_lwIP.tcp.tcp_tmr();
		}

		private static err_t Connected(object arg, tcp_pcb tpcb, err_t err)
		{
			TcpCep _this = (TcpCep)arg;

			if (err != err_t.ERR_OK) {
				err = _this.m_lwIP.tcp.tcp_close(tpcb);
				return err;
			}

			Task wtask = null;
			foreach (Task task in _this.m_SendTaskQueue) {
				if ((task.rtsk.tskwait != TSKWAIT.TTW_TCP) ||
					(task.rtsk.wid != _this.m_CepID))
					continue;

				wtask = task;
				break;
			}

			if (wtask != null) {
				_this.m_SendTaskQueue.Remove(wtask);

				_this.NewSession(tpcb);
				wtask.ReleaseWait();
			}

			return err_t.ERR_OK;
		}

		public ER Shutdown()
		{
			m_Shutdown = true;

			return ER.E_OK;
		}

		public ER Close(TMO tmout)
		{
			err_t err = m_lwIP.tcp.tcp_close(m_Pcb);

			if (err == err_t.ERR_OK)
				return ER.E_OK;
			else
				return ER.E_OBJ;
		}

		public ER SendData(pointer p_data, int len, TMO tmout)
		{
			int result = 0;
			ER ret;

			if (p_data == null)
				return ER.E_PAR;

			if ((m_SendMode != BufferMode.None) && (m_SendMode != BufferMode.NormalCopy))
				return ER.E_OBJ;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			int space = tcp.tcp_sndbuf(m_TPcb);
			if ((m_SendPBuf == null) && (space >= len)) {
				m_SendPBuf = p_data;
				m_SendPos = 0;
				m_SendLen = len;
				SentData(this, m_TPcb, (ushort)space);
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_SendTaskQueue, TSKWAIT.TTW_TCP, m_CepID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_SendPBuf != null)
						return ER.E_RLWAI;

					m_SendPBuf = p_data;
					m_SendPos = 0;
					m_SendLen = len;
					SentData(this, m_TPcb, tcp.tcp_sndbuf(m_TPcb));
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			m_SendMode = BufferMode.NormalCopy;

			return (ER)result;
		}

		private static int CopyToPBuf(pbuf dst, pointer src, int len)
		{
			int pos = 0, rest = len;
			for (pbuf q = dst; q != null; q = q.next) {
				int tmp = rest;
				if (tmp > q.len)
					tmp = q.len;

				pointer.memcpy(q.payload, src + pos, tmp);

				pos += tmp;
				rest -= tmp;
			}

			return pos;
		}

		public ER NetworkToHost(pbuf pk_data)
		{
			if (pk_data == null)
				return ER.E_PAR;

			m_lwIP.pbuf_ref(pk_data);
			m_RecvDataQueue.Enqueue(pk_data);

			if (m_RecvTaskQueue.First != null) {
				Task task = m_RecvTaskQueue.First.Value;
				m_RecvTaskQueue.RemoveFirst();

				if (!task.ReleaseWait())
					return ER.E_RLWAI;
			}
			else {
				m_ctcp.callback(m_CepID, FN.TFN_TCP_RCV_DAT, null);
			}

			return ER.E_OK;
		}

		public ER ReceiveData(pointer p_data, int len, TMO tmout)
		{
			int result = 0;
			ER ret;

			if (p_data == null)
				return ER.E_PAR;

			if ((m_RecvMode != BufferMode.None) && (m_RecvMode != BufferMode.NormalCopy))
				return ER.E_OBJ;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			if ((m_RecvTaskQueue.First == null) && (m_RecvPBuf != null || m_RecvDataQueue.Count != 0)) {
				result = CopyRecvData(p_data, len);
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_RecvTaskQueue, TSKWAIT.TTW_TCP, m_CepID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_RecvDataQueue.Count == 0)
						return ER.E_RLWAI;
					result = CopyRecvData(p_data, len);
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			m_RecvMode = BufferMode.NormalCopy;

			return (ER)result;
		}

		public ER ReceiveDataNblk(pointer p_data, int len)
		{
			int result = 0;

			if (p_data == null)
				return ER.E_PAR;

			if ((m_RecvMode != BufferMode.None) && (m_RecvMode != BufferMode.NormalCopy))
				return ER.E_OBJ;

			result = CopyRecvData(p_data, len);
			if (result == 0)
				return ER.E_WBLK;

			m_RecvMode = BufferMode.NormalCopy;

			return (ER)result;
		}

		private int CopyRecvData(pointer p_data, int len)
		{
			int spos = 0, rest = len;
			int dpos = m_RecvPos;
			pbuf buf = m_RecvPBuf;
			pbuf p = m_RecvPBufPos, q;

			do {
				for (q = p; q != null; q = q.next, dpos = 0) {
					int tmp = dpos + rest;
					if (tmp > q.len)
						tmp = q.len;
					tmp -= dpos;

					pointer.memcpy(p_data + spos, q.payload + dpos, tmp);
					spos += tmp;
					dpos += tmp;
					rest -= tmp;
					if (rest == 0) {
						if (dpos == q.len) {
							q = q.next;
							dpos = 0;
							if (q == null) {
								if (buf != null)
									m_lwIP.pbuf_free(buf);
								buf = p = (m_RecvDataQueue.Count > 0) ? m_RecvDataQueue.Dequeue() : null;
							}
						}
						goto end;
					}
				}
				if (buf != null)
					m_lwIP.pbuf_free(buf);
				buf = p = (m_RecvDataQueue.Count > 0) ? m_RecvDataQueue.Dequeue() : null;
			} while (p != null);

		end:
			m_RecvPBuf = buf;
			m_RecvPBufPos = p;
			m_RecvPos = dpos;

			return spos;
		}

		public ER GetBuffer(ref pointer p_data, TMO tmout)
		{
			int result;
			ER ret;

			//if (p_data == null)
			//	return ER.E_PAR;

			if ((m_SendMode != BufferMode.None) && (m_SendMode != BufferMode.ReduceCopy))
				return ER.E_OBJ;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			// リングバッファが空っぽかループしている場合
			if (m_RCSendWrPos <= m_RCSendRdPos) {
				result = m_RCSendData.Length - 1 - m_RCSendWrPos;
			}
			// リングバッファのループしていない区間
			else {
				result = m_RCSendWrPos - m_RCSendRdPos;
			}

			if ((m_SendTaskQueue.First == null) && (result != 0)) {
				p_data = new pointer(m_RCSendData, m_RCSendWrPos);
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_SendTaskQueue, TSKWAIT.TTW_TCP, m_CepID, tmout);

				switch (ret) {
				case ER.E_OK:
					// リングバッファが空っぽかループしている場合
					if (m_RCSendWrPos <= m_RCSendRdPos) {
						result = m_RCSendData.Length - 1 - m_RCSendWrPos;
					}
					// リングバッファのループしていない区間
					else {
						result = m_RCSendWrPos - m_RCSendRdPos;
					}
					if (result == 0)
						return ER.E_RLWAI;
					p_data = new pointer(m_RCSendData, m_RCSendWrPos);
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			m_SendMode = BufferMode.ReduceCopy;

			return (ER)result;
		}

		public ER SendBuffer(int len)
		{
			if (m_SendMode == BufferMode.ReduceCopy)
				return ER.E_OBJ;

			m_RCSendSdPos = m_RCSendWrPos + len;
			if (m_RCSendSdPos >= m_RCSendData.Length)
				m_RCSendSdPos -= m_RCSendData.Length;

			return ER.E_OK;
		}

		public ER ReceiveBuffer(ref pointer p_data, TMO tmout)
		{
			int result = 0;
			ER ret;

			if (p_data == null)
				return ER.E_PAR;

			if ((m_RecvMode != BufferMode.None) && (m_RecvMode != BufferMode.ReduceCopy))
				return ER.E_OBJ;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			if ((m_RecvTaskQueue.First == null) && (m_RecvPBuf != null || m_RecvDataQueue.Count != 0)) {
				p_data = new pointer(m_RecvPBufPos.payload, m_RecvPos);
				result = m_RecvPBufPos.len - m_RecvPos;
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_RecvTaskQueue, TSKWAIT.TTW_TCP, m_CepID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_RecvDataQueue.Count == 0)
						return ER.E_RLWAI;
					p_data = new pointer(m_RecvPBufPos.payload, m_RecvPos);
					result = m_RecvPBufPos.len - m_RecvPos;
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			m_RecvMode = BufferMode.ReduceCopy;

			return (ER)result;
		}

		public ER ReleaseBuffer(int len)
		{
			if (m_RecvMode == BufferMode.ReduceCopy)
				return ER.E_OBJ;


			return ER.E_OK;
		}

		public ER SendUrgentData(pointer data, int len, TMO tmout)
		{
			throw new NotImplementedException();
		}

		public ER ReceiveUrgentData(pointer data, int len)
		{
			throw new NotImplementedException();
		}

		public ER Cancel(FN fncd)
		{
			switch (fncd) {
			case FN.TFN_TCP_ACP_CEP:
				break;
			case FN.TFN_TCP_CON_CEP:
				break;
			case FN.TFN_TCP_CLS_CEP:
				break;
			case FN.TFN_TCP_SND_DAT:
				break;
			case FN.TFN_TCP_RCV_DAT:
				break;
			case FN.TFN_TCP_GET_BUF:
				break;
			case FN.TFN_TCP_RCV_BUF:
				break;
			case FN.TFN_TCP_SND_OOB:
				break;
			case FN.TFN_TCP_ALL:
				break;
			default:
				return ER.E_PAR;
			}

			return ER.E_OK;
		}

		public ER SetOption(int optname, pointer optval, int optlen)
		{
			throw new NotImplementedException();
		}

		public ER GetOption(int optname, pointer optval, int optlen)
		{
			throw new NotImplementedException();
		}

		public void NewSession(tcp_pcb pcb)
		{
			m_TPcb = pcb;

			tcp.tcp_arg(pcb, this);
			tcp.tcp_recv(pcb, RecvData);
			tcp.tcp_sent(pcb, SentData);
			tcp.tcp_err(pcb, Error);
		}

		private static err_t RecvData(object arg, tcp_pcb tpcb, pbuf p, err_t err)
		{
			TcpCep _this = (TcpCep)arg;

			if (p == null) {
				_this.NetworkToHost(null);
				return err_t.ERR_OK;
			}

			if (err != err_t.ERR_OK) {
				_this.m_lwIP.pbuf_free(p);
				return err;
			}

			_this.NetworkToHost(p);

			_this.m_lwIP.tcp.tcp_recved(tpcb, p.tot_len);

			return err_t.ERR_OK;
		}

		private err_t SentData(object arg, tcp_pcb tpcb, ushort space)
		{
			err_t err = err_t.ERR_OK;
			int rest = space;
			int pos = m_SendPos;
			pointer buf = m_SendPBuf;
			int len = m_SendLen;

			if (buf != null) {
				int tmp = rest;
				if (tmp > len)
					tmp = len;

				err = m_lwIP.tcp.tcp_write(m_TPcb, new pointer(buf, pos), (ushort)tmp, 0);
				if (err != err_t.ERR_OK)
					return err;

				pos += tmp;
				rest -= tmp;
				if (pos == len) {
					pos = 0;
					buf = null;
					len = 0;
			}
			}

			m_SendPBuf = buf;
			m_SendPos = pos;
			m_SendLen = len;

			if ((m_SendPBuf == null) && (rest > 0)) {
				if (m_SendTaskQueue.First != null) {
					Task task = m_SendTaskQueue.First.Value;
					m_SendTaskQueue.RemoveFirst();

					task.ReleaseWait();
				}
			}

			return err;
		}

		private static void Error(object arg, err_t err)
		{
			TcpCep _this = (TcpCep)arg;

			Task wtask = null;
			foreach (Task task in _this.m_SendTaskQueue) {
				if ((task.rtsk.tskwait != TSKWAIT.TTW_TCP) ||
					(task.rtsk.wid != _this.m_CepID))
					continue;

				wtask = task;
				break;
			}

			bool ok = false;
			if (wtask != null) {
				ok = wtask.ReleaseWait();
			}

			if (_this.m_TPcb != null)
				_this.m_lwIP.tcp.tcp_close(_this.m_TPcb);
		}
	}

	internal class TcpRep : StateMachine<bool>
	{
		Nucleus m_Nucleus;
		ID m_RepID;
		T_TCP_CREP m_ctcp;
		TcpCep m_TcpCep;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		LinkedList<tcp_pcb> m_AcceptQueue = new LinkedList<tcp_pcb>();
		Queue<pointer> m_CallBack = new Queue<pointer>();
		lwip m_lwIP;
		tcp_pcb_listen m_Pcb;
		tcp_pcb m_NewPcb;

		public TcpRep(ID tcpid, ref T_TCP_CREP pk_ctcp, Nucleus pNucleus, lwip lwip)
		{
			m_RepID = tcpid;
			m_ctcp = pk_ctcp;
			m_Nucleus = pNucleus;
			m_lwIP = lwip;
		}

		public ID RepID { get { return m_RepID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_TCP_CREP ctcp { get { return m_ctcp; } }

		internal ER Accept(tcp_pcb_listen pcb, TcpCep cep, T_IPV4EP p_dstaddr, TMO tmout)
		{
			ER ret;

			m_Pcb = pcb;
			m_TcpCep = cep;

			tcp.tcp_accept(pcb, Accepting);

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			//if (p_dstaddr == null)
			//	return ER.E_PAR;

			if ((m_TskQueue.First == null) && (m_AcceptQueue.First != null)) {
				m_NewPcb = m_AcceptQueue.First.Value;
				m_AcceptQueue.RemoveFirst();
				p_dstaddr.ipaddr = lwip.lwip_ntohl(m_NewPcb.remote_ip.addr);
				p_dstaddr.portno = lwip.lwip_ntohs(m_NewPcb.remote_port);
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_ACP, m_RepID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_AcceptQueue.First == null)
						return ER.E_RLWAI;
					m_NewPcb = m_AcceptQueue.First.Value;
					m_AcceptQueue.RemoveFirst();
					p_dstaddr.ipaddr = lwip.lwip_ntohl(m_NewPcb.remote_ip.addr);
					p_dstaddr.portno = lwip.lwip_ntohs(m_NewPcb.remote_port);
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		private static err_t Accepting(object arg, tcp_pcb newpcb, err_t err)
		{
			TcpRep _this = (TcpRep)arg;

			_this.m_TcpCep.NewSession(newpcb);

			_this.m_AcceptQueue.AddLast(newpcb);

			if (_this.m_TskQueue.First != null) {
				Task task = _this.m_TskQueue.First.Value;
				_this.m_TskQueue.RemoveFirst();

				if (!task.ReleaseWait())
					return err_t.ERR_WOULDBLOCK;
			}

			return err;
		}
	}
}
