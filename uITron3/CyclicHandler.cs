using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public enum CYCACT
	{
		TCY_OFF = 0,
		TCY_ON = 1,
		TCY_INI = 2,
	}

	public delegate ID TCyclicHandlerFunc();

	public struct T_DCYC
	{
		public object exinf;
		public int cyctim;
		public CYCACT cycact;
		public TCyclicHandlerFunc cychdr;
		public bool cycdel;
	}

	public struct T_RCYC
	{
		public object exinf;
		public long lfttim;
		public CYCACT cycact;
	}

	internal class CyclicHandler
	{
		HNO m_CycNo;
		T_DCYC m_dcyc;
		long m_Count;
		Nucleus m_Nucleus;

		public CyclicHandler(HNO cycno, ref T_DCYC pk_dcyc, Nucleus pNucleus)
		{
			m_CycNo = cycno;
			m_dcyc = pk_dcyc;
			m_Count = pk_dcyc.cyctim;
			m_Nucleus = pNucleus;
		}

		public HNO CycNo { get { return m_CycNo; } }

		public T_DCYC dcyc { get { return m_dcyc; } }

		public long Count { get { return m_Count; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public ER ReferStatus(ref T_RCYC pk_rcyc)
		{
			//if(pk_rcyc == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rcyc.exinf = m_dcyc.exinf;

			// 残り時間
			pk_rcyc.lfttim = m_Count;

			// 現在の活性状態
			pk_rcyc.cycact = m_dcyc.cycact;

			return ER.E_OK;
		}

		public bool OnTime(long Interval)
		{
			ID tskID;
			bool retry = false;

			if ((m_dcyc.cycact & CYCACT.TCY_INI) != 0) {
				m_Count = m_dcyc.cyctim;
				m_dcyc.cycact &= ~CYCACT.TCY_INI;
			}
			if ((m_dcyc.cycact & CYCACT.TCY_ON) != 0) {
				m_Count -= Interval;
				if (m_Count <= 0) {
					m_Count += m_dcyc.cyctim;
					if (m_Count <= 0) {
						retry = true;
					}
					try {
						tskID = m_dcyc.cychdr();
					}
					catch (Exception) {
						tskID = ID.TSK_NULL;
					}
					if (tskID != ID.TSK_NULL) {
						Task Task;

						Task = m_Nucleus.GetTask(tskID);
						if (Task != null) {
							Task.Wakeup();
						}
					}
				}
			}

			return retry;
		}

		public void Redefine(ref T_DCYC pk_dcyc)
		{
			m_dcyc = pk_dcyc;
			m_Count = pk_dcyc.cyctim;
		}

		public ER Activate(CYCACT cycact)
		{
			m_dcyc.cycact = cycact;

			return ER.E_OK;
		}
	}
}
