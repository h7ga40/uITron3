using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct FLGPTN
	{
		public uint Value;

		public FLGPTN(uint value)
		{
			Value = value;
		}

		public static implicit operator uint(FLGPTN a)
		{
			return a.Value;
		}

		public static implicit operator FLGPTN(uint a)
		{
			return new FLGPTN(a);
		}
	}

	public enum MODE
	{
		TWF_ANDW = 0,
		TWF_CLR = 1,
		TWF_ORW = 2,
	}

	public struct T_CFLG
	{
		public object exinf;
	}

	public struct T_RFLG
	{
		public object exinf;
		public bool wtsk;
		public FLGPTN flgptn;
	}

	internal class EventFlag
	{
		ID m_FlgID;
		T_CFLG m_cflg;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		FLGPTN m_FlagPattern;
		Nucleus m_Nucleus;

		public EventFlag(ID flgid, ref T_CFLG pk_cflg, Nucleus pNucleus)
		{
			m_FlgID = flgid;
			m_cflg = pk_cflg;
			m_Nucleus = pNucleus;
		}

		public ID FlgID { get { return m_FlgID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CFLG cflg { get { return m_cflg; } }

		public ER ReferStatus(ref T_RFLG pk_rflg)
		{
			//if(pk_rflg == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rflg.exinf = m_cflg.exinf;

			// 待ちタスクの有無
			pk_rflg.wtsk = m_TskQueue.First != null;

			// 待ちイベントフラグの有無
			pk_rflg.flgptn = m_FlagPattern;

			return ER.E_OK;
		}

		private bool CheckPattern(FLGPTN waiptn, MODE wfmode)
		{
			if ((wfmode & MODE.TWF_ORW) != 0)
				return (m_FlagPattern & waiptn) != 0;
			else
				return (m_FlagPattern & waiptn) == 0;
		}

		public ER SetEventFlag(FLGPTN setptn)
		{
			if (setptn.Value == 0)
				return ER.E_PAR;

			m_FlagPattern |= setptn;

			for (LinkedListNode<Task> node = m_TskQueue.First; node != null; node = node.Next) {
				Task task = node.Value;
				if (CheckPattern(task.WaitPattern, task.WaitMode)) {
					m_TskQueue.Remove(node);

					if (!task.ReleaseWait())
						return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		public ER ClearEventFlag(FLGPTN setptn)
		{
			if (setptn.Value == 0)
				return ER.E_PAR;

			m_FlagPattern &= ~setptn;

			for (LinkedListNode<Task> node = m_TskQueue.First; node != null; node = node.Next) {
				Task task = node.Value;
				if (CheckPattern(task.WaitPattern, task.WaitMode)) {
					m_TskQueue.Remove(node);

					if (!task.ReleaseWait())
						return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		public ER WaitEventFlag(ref FLGPTN p_flgptn, TMO tmout, FLGPTN waiptn, MODE wfmode)
		{
			ER ret;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			//if (p_flgptn == null)
			//	return ER.E_PAR;

			if ((wfmode & ~(MODE.TWF_ANDW | MODE.TWF_ORW | MODE.TWF_CLR)) != 0)
				return ER.E_PAR;

			if ((m_TskQueue.First == null) && CheckPattern(waiptn, wfmode)) {
				p_flgptn = m_FlagPattern;
				if ((wfmode & MODE.TWF_CLR) != 0)
					m_FlagPattern = 0;
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				task.WaitPattern = waiptn;
				task.WaitMode = wfmode;
				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_FLG, m_FlgID, tmout);

				switch (ret) {
				case ER.E_OK:
					p_flgptn = m_FlagPattern;
					if (!CheckPattern(waiptn, wfmode))
						return ER.E_RLWAI;
					if ((wfmode & MODE.TWF_CLR) != 0)
						m_FlagPattern = 0;
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}
	}
}
