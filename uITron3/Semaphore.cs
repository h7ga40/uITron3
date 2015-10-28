using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct T_CSEM
	{
		public object exinf;
		public int isemcnt;
		public int maxsem;
	}

	public struct T_RSEM
	{
		public object exinf;
		public bool wtsk;
		public int semcnt;
		public int maxsem;
	}

	internal class Semaphore
	{
		ID m_SemID;
		T_CSEM m_csem;
		int m_Count;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		Nucleus m_Nucleus;

		public Semaphore(ID semid, ref T_CSEM pk_csem, Nucleus pNucleus)
		{
			m_SemID = semid;
			m_csem = pk_csem;
			m_Count = pk_csem.isemcnt;
			m_Nucleus = pNucleus;
		}

		public ID SemID { get { return m_SemID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CSEM csem { get { return m_csem; } }

		public ER ReferStatus(ref T_RSEM pk_rsem)
		{
			//if (pk_rsem == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rsem.exinf = m_csem.exinf;

			// 待ちタスクの有無
			pk_rsem.wtsk = m_TskQueue.First != null;

			// 現在の資源数
			pk_rsem.semcnt = m_Count;

			// 最大資源数
			pk_rsem.maxsem = m_csem.maxsem;

			return ER.E_OK;
		}

		public ER Wait(TMO tmout)
		{
			ER ret;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			m_Count--;

			if (m_Count < 0) {
				if (tmout == 0) {
					m_Count++;
					return ER.E_TMOUT;
				}

				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_SEM, m_SemID, tmout);

				switch (ret) {
				case ER.E_OK:
					return ER.E_OK;
				case ER.E_TMOUT:
					m_Count++;
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		public ER Signal()
		{
			if (m_TskQueue.First != null) {
				Task task = m_TskQueue.First.Value;
				m_TskQueue.RemoveFirst();

				if (!task.ReleaseWait())
					return ER.E_RLWAI;
			}
			else {
				m_Count++;
				if (m_Count > m_csem.maxsem)
					return ER.E_QOVR;
			}

			return ER.E_OK;
		}
	}
}
