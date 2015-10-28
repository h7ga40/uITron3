using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public enum TSKSTAT
	{
		/// <summary>実行状態</summary>
		TTS_RUN = 0x01,
		/// <summary>実行可能状態</summary>
		TTS_RDY = 0x02,
		/// <summary>待ち状態</summary>
		TTS_WAI = 0x04,
		/// <summary>強制待ち状態</summary>
		TTS_SUS = 0x08,
		/// <summary>二重待ち状態</summary>
		TTS_WAS = 0x0c,
		/// <summary>休止状態</summary>
		TTS_DMT = 0x10,
	}

	public enum TSKWAIT
	{
		/// <summary>起床待ち状態</summary>
		TTW_SLP = 0x0001,
		/// <summary>時間経過待ち状態</summary>
		TTW_DLY = 0x0002,
		/// <summary>イベント・フラグ待ち状態</summary>
		TTW_FLG = 0x0010,
		/// <summary>セマフォ資源待ち状態</summary>
		TTW_SEM = 0x0020,
		/// <summary>メッセージ待ち状態</summary>
		TTW_MBX = 0x0040,
		/// <summary>メモリ・ブロック待ち状態</summary>
		TTW_MPL = 0x1000,
		/// <summary>固定長メモリ・ブロック待ち状態</summary>
		TTW_MPF = 0x2000,
		/// <summary>TCP受付待ち状態</summary>
		TTW_ACP = 0x00010000,
		/// <summary>TCPデータ受信待ち状態</summary>
		TTW_TCP = 0x00020000,
		/// <summary>TCPデータ受信待ち状態</summary>
		TTW_UDP = 0x00040000,
	}

	public delegate void TTaskMain(object stdcd);

	public struct T_CTSK
	{
		public object exinf;
		public TSKSTAT tskstat;
		public PRI itskpri;
		public TTaskMain task;
	}

	public struct T_RTSK
	{
		public object exinf;
		public TSKSTAT tskstat;
		public PRI tskpri;
		public int wupcnt;
		public TSKWAIT tskwait;
		public ID wid;
		public int suscnt;
	}

	internal class Task : StateMachine<bool>
	{
		ID m_TaskID;
		T_CTSK m_ctsk;
		T_RTSK m_rtsk = new T_RTSK();
		Nucleus m_Nucleus;
		ICPUContext m_CPUContext;
		bool m_ReleaseWait;

		public Task(ID TaskID, ref T_CTSK pk_ctsk, Nucleus pNucleus)
		{
			StaCD = null;
			m_TaskID = TaskID;

			m_ctsk = pk_ctsk;
			m_rtsk.exinf = m_ctsk.exinf;
			m_rtsk.tskpri = m_ctsk.itskpri;
			m_rtsk.tskstat = TSKSTAT.TTS_DMT;

			m_Nucleus = pNucleus;

			m_CPUContext = m_Nucleus.NewCPUContext();
			m_ReleaseWait = false;
		}

		public object StaCD;

		public ID TaskID { get { return m_TaskID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CTSK ctsk { get { return m_ctsk; } }

		public T_RTSK rtsk { get { return m_rtsk; } }

		public int MemoryBlockSize;

		public FLGPTN WaitPattern;

		public MODE WaitMode;

		public ICPUContext GetCPUContext() { return m_CPUContext; }

		~Task()
		{
		}

		protected void Execute()
		{
			try {
				if (m_ctsk.task != null)
					m_ctsk.task(StaCD);
			}
			finally {
				m_Nucleus.LockCPU();
				try {
					if (m_rtsk.tskstat != TSKSTAT.TTS_DMT) {
						m_rtsk.tskstat = TSKSTAT.TTS_DMT;
						if (m_Nucleus.m_CurrentTask == this) {
							m_Nucleus.m_CurrentTask = null;
						}
						m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].Remove(this);
						m_Nucleus.Dispatch();
					}
				}
				finally {
					m_Nucleus.UnlockCPU();
				}
			}
		}

		protected void OnTimeOut(object Data)
		{
			LinkedList<Task> WaitQueue = (LinkedList<Task>)Data;

			WaitQueue.Remove(this);

			if (m_rtsk.tskstat == TSKSTAT.TTS_WAI) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask != this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_WAS) {
				m_rtsk.tskstat = TSKSTAT.TTS_SUS;
			}
			else {
				throw new Exception();
			}

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].AddLast(this);

			// タイムアウト処理待ち
			SetState(true);
		}

		public ER ReferStatus(ref T_RTSK pk_rtsk)
		{
			//if (pk_rtsk == null)
			//	return ER.E_PAR;

			pk_rtsk = m_rtsk;

			return ER.E_OK;
		}

		public bool ReleaseWait()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_WAI) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask != this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_WAS) {
				m_rtsk.tskstat = TSKSTAT.TTS_SUS;
			}
			else {
				return false;
			}

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].AddLast(this);

			return m_Nucleus.Dispatch();
		}

		public void Ready()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_RUN) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
				m_Nucleus.m_CurrentTask = null;
			}
			else if ((m_rtsk.tskstat != TSKSTAT.TTS_WAI) && (m_rtsk.tskstat != TSKSTAT.TTS_WAS)) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask != this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
			}
			else {
				throw new Exception();
			}
		}

		public void Run()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_RDY) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == null);
				m_rtsk.tskstat = TSKSTAT.TTS_RUN;
				m_Nucleus.m_CurrentTask = this;
			}
			else {
				throw new Exception();
			}
		}

		public ER Wait(LinkedList<Task> WaitQueue, TSKWAIT tskwait, ID wid, TMO tmout)
		{
			ER Result;

			System.Diagnostics.Debug.Assert(m_CPUContext.IsCurrent());

			if (m_rtsk.tskstat == TSKSTAT.TTS_SUS) {
				m_rtsk.tskstat = TSKSTAT.TTS_WAS;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_RUN) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == this);
				m_rtsk.tskstat = TSKSTAT.TTS_WAI;
				m_Nucleus.m_CurrentTask = null;
			}
			else {
				m_rtsk.tskstat = TSKSTAT.TTS_WAI;
			}
			m_rtsk.tskwait = tskwait;
			m_rtsk.wid = wid;

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].Remove(this);

			// 待ちタスクキューへ追加
			WaitQueue.AddLast(this);

			// タイマー設定
			SetState(false, tmout, WaitQueue, OnTimeOut);

			// タスクスケジューリング
			if (m_CPUContext.Dispatch()) {
				if (State)
					Result = ER.E_TMOUT;
				else
					Result = ER.E_OK;
			}
			else {
				m_rtsk.tskstat = TSKSTAT.TTS_RUN;
				m_ReleaseWait = true;
				Result = ER.E_RLWAI;
			}

			return Result;
		}

		public ER Suspend()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_DMT)
				return ER.E_OBJ;

			m_rtsk.suscnt++;
			if (m_rtsk.suscnt > 127)
				return ER.E_QOVR;

			if (m_rtsk.tskstat == TSKSTAT.TTS_WAI) {
				m_rtsk.tskstat = TSKSTAT.TTS_WAS;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_RUN) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == this);
				m_rtsk.tskstat = TSKSTAT.TTS_SUS;
				m_Nucleus.m_CurrentTask = null;
			}
			else if (m_rtsk.tskstat != TSKSTAT.TTS_WAS) {
				m_rtsk.tskstat = TSKSTAT.TTS_SUS;
			}

			return ER.E_NOSPT;
		}

		public ER Resume()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_DMT)
				return ER.E_OBJ;

			if (m_rtsk.suscnt > 0)
				m_rtsk.suscnt--;

			if (m_rtsk.tskstat == TSKSTAT.TTS_WAS) {
				m_rtsk.tskstat = TSKSTAT.TTS_WAI;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_RUN) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
				m_Nucleus.m_CurrentTask = null;
			}
			else if (m_rtsk.tskstat != TSKSTAT.TTS_WAI) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask != this);
				m_rtsk.tskstat = TSKSTAT.TTS_RDY;
			}

			return ER.E_NOSPT;
		}

		public ER ForceResume()
		{
			m_rtsk.suscnt = 0;
			Resume();

			return ER.E_NOSPT;
		}

		public void Prepare()
		{
			m_CPUContext.Activate(this, Execute);
		}

		public ER Start(int stacd)
		{
			if (m_rtsk.tskstat != TSKSTAT.TTS_DMT) {
				return ER.E_OBJ;
			}

			StaCD = stacd;

			System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask != this);
			m_rtsk.tskstat = TSKSTAT.TTS_RDY;

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].AddLast(this);

			if (!m_Nucleus.Dispatch()) {
				return ER.E_OBJ;
			}

			return ER.E_OK;
		}

		public void Exit()
		{
			if (m_ReleaseWait) {
				m_rtsk.tskstat = TSKSTAT.TTS_DMT;
				if (m_Nucleus.m_CurrentTask == this)
					m_Nucleus.m_CurrentTask = null;
				m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].Remove(this);
				m_Nucleus.ExitAndDispatch();
				return;
			}
			else if (m_rtsk.tskstat == TSKSTAT.TTS_RUN) {
				System.Diagnostics.Debug.Assert(m_Nucleus.m_CurrentTask == this);
				m_rtsk.tskstat = TSKSTAT.TTS_DMT;
				m_Nucleus.m_CurrentTask = null;
			}
			else {
				throw new Exception();
			}

			if (this == null)
				throw new Exception();

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].Remove(this);

			m_Nucleus.ExitAndDispatch();
		}

		public ER Sleep(TMO tmout)
		{
			ER ret;

			if (tmout == TMO.TMO_POL) {
				if (m_rtsk.wupcnt > 0)
					m_rtsk.wupcnt--;

				return ER.E_OK;
			}

			if (m_rtsk.wupcnt <= 0) {
				ret = Wait(m_Nucleus.m_SlpTskQ, TSKWAIT.TTW_SLP, new ID(0), tmout);

				switch (ret) {
				case ER.E_OK:
					m_rtsk.wupcnt = 0;
					return ER.E_OK;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}
			else {
				m_rtsk.wupcnt--;

				return ER.E_OK;
			}
		}

		public ER Wakeup()
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_DMT)
				return ER.E_OBJ;

			m_rtsk.wupcnt++;
			if (m_rtsk.wupcnt > 127)
				return ER.E_QOVR;

			if (this == null)
				return ER.E_OK;

			m_Nucleus.m_SlpTskQ.Remove(this);

			if (!ReleaseWait())
				return ER.E_RLWAI;

			return ER.E_OK;
		}

		public ER CanWakeup(ref int p_wupcnt)
		{
			if (m_rtsk.tskstat == TSKSTAT.TTS_DMT)
				return ER.E_OBJ;

			p_wupcnt = m_rtsk.wupcnt;

			m_rtsk.wupcnt = 0;

			return ER.E_OK;
		}

		public ER ChangePriority(PRI tskpri)
		{
			if ((tskpri < 0) || (tskpri >= Itron.TASK_PRI_NUM))
				return ER.E_PAR;

			if (m_rtsk.tskstat == TSKSTAT.TTS_DMT)
				return ER.E_OBJ;

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].Remove(this);

			if (tskpri == PRI.TPRI_INI) {
				m_rtsk.tskpri = m_ctsk.itskpri;
			}
			else {
				m_rtsk.tskpri = tskpri;
			}

			m_Nucleus.m_ReadyQueue[m_rtsk.tskpri].AddLast(this);

			if (!m_Nucleus.Dispatch())
				return ER.E_RLWAI;

			return ER.E_OK;
		}

		public ER Terminate()
		{
			return ER.E_NOSPT;
		}

		public ER Delay(DLYTIME dlytim)
		{
			return ER.E_NOSPT;
		}

		public ER RotateReadyQueue(PRI tskpri)
		{
			return ER.E_NOSPT;
		}
	}
}
