using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uITron3
{
	internal interface IKernel
	{
		ICPUContext GetCurrent();
		ICPUContext NewCPUContext();
		void LockCPU();
		void UnlockCPU();
		bool Dispatch();
		bool ExitAndDispatch();
	}

	internal class Nucleus
	{
		IKernel m_Kernel;
		int m_SysTmrIntNo;
		TMO m_SysTmrIntv;
		T_RSYS m_rsys;
		SYSTIME m_SysTime;
		internal LinkedList<Task>[] m_ReadyQueue = new LinkedList<Task>[Itron.TASK_PRI_NUM];
		internal LinkedList<Task> m_SlpTskQ = new LinkedList<Task>();
		internal Task m_CurrentTask;
		Task m_ScheduledTask;
		List<Task> m_TaskTable = new List<Task>();
		List<MemoryPool> m_MemPoolTable = new List<MemoryPool>();
		List<MemoryPoolFixedsize> m_MemPoolFxTable = new List<MemoryPoolFixedsize>();
		List<EventFlag> m_EventFlagTable = new List<EventFlag>();
		List<Mailbox> m_MailboxTable = new List<Mailbox>();
		List<Semaphore> m_SemaphoreTable = new List<Semaphore>();
		List<CyclicHandler> m_CyclicHandlerTable = new List<CyclicHandler>();
		lwip m_lwIP;
		List<UdpCep> m_UdpCepTable = new List<UdpCep>();
		List<TcpCep> m_TcpCepTable = new List<TcpCep>();
		List<TcpRep> m_TcpRepTable = new List<TcpRep>();
		long m_Lock;

		public Nucleus(IKernel kernel, int sysTmrIntNo, TMO sysTmrIntv)
		{
			m_Kernel = kernel;
			m_SysTmrIntNo = sysTmrIntNo;
			m_SysTmrIntv = sysTmrIntv;

			m_rsys.sysstat = SYSSTAT.TTS_INDP;

			for (int i = 0; i < m_ReadyQueue.Length; i++)
				m_ReadyQueue[i] = new LinkedList<Task>();

			m_SysTime.Value = 0;
			m_CurrentTask = null;
			m_lwIP = new lwip();
		}

		public int SysTmrIntNo { get { return m_SysTmrIntNo; } }

		public Task GetScheduledTask() { return m_ScheduledTask; }

		public Task GetCurrentTask() { return m_CurrentTask; }

		public ICPUContext NewCPUContext() { return m_Kernel.NewCPUContext(); }

		public void LockCPU() { m_Kernel.LockCPU(); }

		public void UnlockCPU() { m_Kernel.UnlockCPU(); }

		public bool Dispatch() { return m_Kernel.Dispatch(); }

		public bool ExitAndDispatch() { return m_Kernel.ExitAndDispatch(); }

		public void Start()
		{
			ip_addr ipaddr = new ip_addr(0xC0B60102);
			ip_addr netmask = new ip_addr(0xFFFFFF00);
			ip_addr gw = new ip_addr(0xC0B60101);

#if LWIP_STATS
			stats.stats_init();
#endif
			sys.sys_init(m_lwIP);

			m_lwIP.mem_init();
			lwip.memp_init();
			pbuf.pbuf_init(m_lwIP);

			ip.ip_init(m_lwIP);

			udp.udp_init(m_lwIP);
			tcp.tcp_init(m_lwIP);

			netif.netif_init(m_lwIP, "", ipaddr, netmask, gw, netif_output);
		}

		private void netif_output(netif netif, byte[] packet, ip_addr src, ip_addr dest, byte proto)
		{
			throw new NotImplementedException();
		}

		public void OnSysTime()
		{
			bool Retry = false;

			m_SysTime.Value += m_SysTmrIntv;

			do {
				foreach (CyclicHandler Cyc in m_CyclicHandlerTable) {
					if (Cyc != null) {
						Retry = Cyc.OnTime(m_SysTmrIntv) || Retry;
					}
				}
			} while (Retry);

			foreach (Task Task in m_TaskTable) {
				if ((Task != null)
					&& ((Task.rtsk.tskstat == TSKSTAT.TTS_WAI) || (Task.rtsk.tskstat == TSKSTAT.TTS_WAS))) {
					Task.Progress(m_SysTmrIntv);
				}
			}

			foreach (UdpCep UdpCep in m_UdpCepTable) {
				if ((UdpCep != null) && (UdpCep.State)) {
					UdpCep.Progress(m_SysTmrIntv);
				}
			}

			foreach (TcpCep TcpCep in m_TcpCepTable) {
				if ((TcpCep != null) && (TcpCep.State)) {
					TcpCep.Progress(m_SysTmrIntv);
				}
			}

			foreach (Task Task in m_TaskTable) {
				if ((Task != null)
					&& ((Task.rtsk.tskstat == TSKSTAT.TTS_WAI) || (Task.rtsk.tskstat == TSKSTAT.TTS_WAS))) {
					Task.CallTimeOut();
				}
			}

			foreach (UdpCep UdpCep in m_UdpCepTable) {
				if ((UdpCep != null) && (UdpCep.State)) {
					UdpCep.CallTimeOut();
				}
			}

			foreach (TcpCep TcpCep in m_TcpCepTable) {
				if ((TcpCep != null) && (TcpCep.State)) {
					TcpCep.CallTimeOut();
				}
			}
		}

		public void Scheduling()
		{
			int i;
			LinkedList<Task> Queue;
			LinkedListNode<Task> Node;

			m_ScheduledTask = null;
			for (i = 0; i < Itron.TASK_PRI_NUM; i++) {
				Queue = m_ReadyQueue[i];
				Node = Queue.First;
				if (Node != null) {
					m_ScheduledTask = Node.Value;
					break;
				}
			}
		}

		public ER CreateTask(ID tskid, ref T_CTSK pk_ctsk, out ID p_takid)
		{
			int i;
			Task Task;

			//if(pk_ctsk == null)
			//	return ER.E_PAR;

			if (tskid == ID.ID_AUTO) {
				//if (p_takid == null)
				//	return ER.E_PAR;

				tskid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_TaskTable.Count) {
						Task = new Task(tskid, ref pk_ctsk, this);
						m_TaskTable.Add(Task);
						Task.Prepare();
						break;
					}

					if (tskid == m_TaskTable[i].TaskID) {
						tskid.Value++;
					}
					else {
						Task = new Task(tskid, ref pk_ctsk, this);
						m_TaskTable.Insert(i, Task);
						Task.Prepare();
						break;
					}
				}
				p_takid = tskid;
			}
			else {
				ID tmpid;

				p_takid = ID.NULL;

				for (i = 0; i < m_TaskTable.Count; i++) {
					tmpid = m_TaskTable[i].TaskID;

					if (tskid == tmpid) {
						return ER.E_OBJ;
					}
					else if (tskid < tmpid) {
						break;
					}
				}
				Task = new Task(tskid, ref pk_ctsk, this);
				m_TaskTable.Insert(i, Task);
				Task.Prepare();
			}

			return ER.E_OK;
		}

		public ER DeleteTask(ID tskid)
		{
			ER ret = ER.E_OBJ;
			int i;
			ID tmpid;
			Task Task;

			LockTaskTable();
			try {
				for (i = 0; i < m_TaskTable.Count; i++) {
					Task = m_TaskTable[i];
					tmpid = Task.TaskID;

					if (tskid == tmpid) {
						m_TaskTable.RemoveAt(i);

						//delete Task;

						ret = ER.E_OK;
						break;
					}
					else if (tskid < tmpid) {
						break;
					}
				}
			}
			finally {
				UnlockTaskTable();
			}

			return ret;
		}

		public Task GetTask(ID tskid)
		{
			Task Result = null;
			Task Task;

			// 実行中のタスクの場合
			if (tskid == ID.TSK_SELF) {
#if false // カーネルでもタスクでもないスレッドから呼ばれた場合、この処理では実行中のタスクを返してしまう
				// 今実行しているスレッドと、実行中と設定されているタスクが同じ場合
				if ((m_CurrentTask == null) || m_CurrentTask.GetCPUContext().IsCurrent()) {
					// 実行中と設定されているタスクを返す
					return m_CurrentTask;
				}

				// 今実行しているスレッドと、実行中と設定されているタスクが違う場合
				LockTaskTable();
				try {
					// 今実行しているスレッドが、管理しているタスクの一つだった場合
					for (int i = 0; i < m_TaskTable.Count; i++) {
						Task = m_TaskTable[i];
						if (Task.GetCPUContext().IsCurrent()) {
							// m_CurrentTaskの設定ミスあり
							//System.Diagnostics.Debug.Assert(false);
							Result = Task;
							break;
						}
					}
				}
				finally {
					UnlockTaskTable();
				}

				// m_CurrentTaskの設定にミスがなければカーネルモード扱い
				return Result;
#else
				ICPUContext context = m_Kernel.GetCurrent();
				if (context == null)
					return null;
				return context.GetTask();
#endif
			}

			LockTaskTable();
			try {
				// タスクID指定の場合
				for (int i = 0; i < m_TaskTable.Count; i++) {
					Task = m_TaskTable[i];
					if (tskid == Task.TaskID) {
						Result = Task;
						break;
					}
				}
			}
			finally {
				UnlockTaskTable();
			}

			return Result;
		}

		public ER CreateMemoryPool(ID mplid, ref T_CMPL pk_cmpl, out ID p_mplid)
		{
			int i;
			MemoryPool Mpl = null;

			//if (pk_cmpl == null)
			//	return ER.E_PAR;

			if (mplid == ID.ID_AUTO) {
				//if (p_mplid == 0)
				//	return ER.E_PAR;

				mplid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_MemPoolTable.Count) {
						Mpl = new MemoryPool(mplid, ref pk_cmpl, this);
						m_MemPoolTable.Add(Mpl);
						break;
					}

					if (mplid == m_MemPoolTable[i].MplID) {
						mplid.Value++;
					}
					else {
						Mpl = new MemoryPool(mplid, ref pk_cmpl, this);
						m_MemPoolTable.Insert(i, Mpl);
						break;
					}
				}
				p_mplid = mplid;
			}
			else {
				ID tmpid;

				p_mplid = ID.NULL;

				for (i = 0; i < m_MemPoolTable.Count; i++) {
					tmpid = m_MemPoolTable[i].MplID;

					if (mplid == tmpid) {
						return ER.E_OBJ;
					}
					else if (mplid < tmpid) {
						break;
					}
				}
				Mpl = new MemoryPool(mplid, ref pk_cmpl, this);
				m_MemPoolTable.Insert(i, Mpl);
			}

			return ER.E_OK;
		}

		public ER DeleteMemoryPool(ID mplid)
		{
			int i;
			ID tmpid;

			for (i = 0; i < m_MemPoolTable.Count; i++) {
				tmpid = m_MemPoolTable[i].MplID;

				if (mplid == tmpid) {
					m_MemPoolTable.RemoveAt(i);

					return ER.E_OK;
				}
				else if (mplid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public MemoryPool GetMemoryPool(ID mplid)
		{
			int i;
			MemoryPool Result;

			for (i = 0; i < m_MemPoolTable.Count; i++) {
				Result = m_MemPoolTable[i];
				if (mplid == Result.MplID) {
					return Result;
				}
			}

			return null;
		}

		public ER CreateMemoryPoolFixedsize(ID mpfid, ref T_CMPF pk_cmpf, out ID p_mpfid)
		{
			int i;
			MemoryPoolFixedsize Mpf = null;

			//if (pk_cmpf == null)
			//	return ER.E_PAR;

			if (mpfid == ID.ID_AUTO) {
				//if (p_mpfid == 0)
				//	return ER.E_PAR;

				mpfid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_MemPoolFxTable.Count) {
						Mpf = new MemoryPoolFixedsize(mpfid, ref pk_cmpf, this);
						m_MemPoolFxTable.Add(Mpf);
						break;
					}

					if (mpfid == m_MemPoolFxTable[i].MpfID) {
						mpfid.Value++;
					}
					else {
						Mpf = new MemoryPoolFixedsize(mpfid, ref pk_cmpf, this);
						m_MemPoolFxTable.Insert(i, Mpf);
						break;
					}
				}
				p_mpfid = mpfid;
			}
			else {
				ID tmpid;

				p_mpfid = ID.NULL;

				for (i = 0; i < m_MemPoolFxTable.Count; i++) {
					tmpid = m_MemPoolFxTable[i].MpfID;

					if (mpfid == tmpid) {
						return ER.E_OBJ;
					}
					else if (mpfid < tmpid) {
						break;
					}
				}
				Mpf = new MemoryPoolFixedsize(mpfid, ref pk_cmpf, this);
				m_MemPoolFxTable.Insert(i, Mpf);
			}

			return ER.E_OK;
		}

		public ER DeleteMemoryPoolFixedsize(ID mpfid)
		{
			int i;
			ID tmpid;

			for (i = 0; i < m_MemPoolFxTable.Count; i++) {
				tmpid = m_MemPoolFxTable[i].MpfID;

				if (mpfid == tmpid) {
					m_MemPoolFxTable.RemoveAt(i);

					return ER.E_OK;
				}
				else if (mpfid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public MemoryPoolFixedsize GetMemoryPoolFixedsize(ID mpfid)
		{
			int i;
			MemoryPoolFixedsize Result;

			for (i = 0; i < m_MemPoolFxTable.Count; i++) {
				Result = m_MemPoolFxTable[i];
				if (mpfid == Result.MpfID) {
					return Result;
				}
			}

			return null;
		}

		public ER CreateEventFlag(ID flgid, ref T_CFLG pk_cflg, out ID p_flgid)
		{
			int i;

			//if (pk_cflg == null)
			//	return ER.E_PAR;

			if (flgid == ID.ID_AUTO) {
				//if (p_flgid == null)
				//	return ER.E_PAR;

				flgid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_EventFlagTable.Count) {
						m_EventFlagTable.Add(new EventFlag(flgid, ref pk_cflg, this));
						break;
					}

					if (flgid == m_EventFlagTable[i].FlgID) {
						flgid.Value++;
					}
					else {
						m_EventFlagTable.Insert(i, new EventFlag(flgid, ref pk_cflg, this));
						break;
					}
				}
				p_flgid = flgid;
			}
			else {
				ID tmpid;

				p_flgid = ID.NULL;

				for (i = 0; i < m_EventFlagTable.Count; i++) {
					tmpid = m_EventFlagTable[i].FlgID;

					if (flgid == tmpid) {
						return ER.E_OBJ;
					}
					else if (flgid < tmpid) {
						break;
					}
				}
				m_EventFlagTable.Insert(i, new EventFlag(flgid, ref pk_cflg, this));
			}

			return ER.E_OK;
		}

		public ER DeleteEventFlag(ID flgid)
		{
			int i;
			ID tmpid;
			EventFlag EventFlag;

			for (i = 0; i < m_EventFlagTable.Count; i++) {
				EventFlag = m_EventFlagTable[i];
				tmpid = EventFlag.FlgID;

				if (flgid == tmpid) {
					m_EventFlagTable.RemoveAt(i);

					//delete EventFlag;

					return ER.E_OK;
				}
				else if (flgid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public EventFlag GetEventFlag(ID flgid)
		{
			int i;
			EventFlag Result;

			for (i = 0; i < m_EventFlagTable.Count; i++) {
				Result = m_EventFlagTable[i];
				if (flgid == Result.FlgID) {
					return Result;
				}
			}

			return null;
		}

		public ER CreateMailbox(ID mbxid, ref T_CMBX pk_cmbx, out ID p_mbxid)
		{
			int i;

			//if (pk_cmbx == null)
			//	return ER.E_PAR;

			if (mbxid == ID.ID_AUTO) {
				//if (p_mbxid == 0)
				//	return ER.E_PAR;

				mbxid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_MailboxTable.Count) {
						m_MailboxTable.Add(new Mailbox(mbxid, ref pk_cmbx, this));
						break;
					}

					if (mbxid == m_MailboxTable[i].MbxID) {
						mbxid.Value++;
					}
					else {
						m_MailboxTable.Insert(i, new Mailbox(mbxid, ref pk_cmbx, this));
						break;
					}
				}
				p_mbxid = mbxid;
			}
			else {
				ID tmpid;

				p_mbxid = ID.NULL;

				for (i = 0; i < m_MailboxTable.Count; i++) {
					tmpid = m_MailboxTable[i].MbxID;

					if (mbxid == tmpid) {
						return ER.E_OBJ;
					}
					else if (mbxid < tmpid) {
						break;
					}
				}
				m_MailboxTable.Insert(i, new Mailbox(mbxid, ref pk_cmbx, this));
			}

			return ER.E_OK;
		}

		public ER DeleteMailbox(ID mbxid)
		{
			int i;
			ID tmpid;
			Mailbox Mailbox;

			for (i = 0; i < m_MailboxTable.Count; i++) {
				Mailbox = m_MailboxTable[i];
				tmpid = Mailbox.MbxID;

				if (mbxid == tmpid) {
					m_MailboxTable.RemoveAt(i);

					//delete Mailbox;

					return ER.E_OK;
				}
				else if (mbxid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public Mailbox GetMailbox(ID mbxid)
		{
			int i;
			Mailbox Result;

			for (i = 0; i < m_MailboxTable.Count; i++) {
				Result = m_MailboxTable[i];
				if (mbxid == Result.MbxID) {
					return Result;
				}
			}

			return null;
		}

		public ER CreateSemaphore(ID semid, ref T_CSEM pk_csem, out ID p_semid)
		{
			int i;

			//if (pk_csem == null)
			//	return ER.E_PAR;

			if (semid == ID.ID_AUTO) {
				//if (p_semid == 0)
				//	return ER.E_PAR;

				semid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_SemaphoreTable.Count) {
						m_SemaphoreTable.Add(new Semaphore(semid, ref pk_csem, this));
						break;
					}

					if (semid == m_SemaphoreTable[i].SemID) {
						semid.Value++;
					}
					else {
						m_SemaphoreTable.Insert(i, new Semaphore(semid, ref pk_csem, this));
						break;
					}
				}
				p_semid = semid;
			}
			else {
				ID tmpid;

				p_semid = ID.NULL;

				for (i = 0; i < m_SemaphoreTable.Count; i++) {
					tmpid = m_SemaphoreTable[i].SemID;

					if (semid == tmpid) {
						return ER.E_OBJ;
					}
					else if (semid < tmpid) {
						break;
					}
				}
				m_SemaphoreTable.Insert(i, new Semaphore(semid, ref pk_csem, this));
			}

			return ER.E_OK;
		}

		public ER DeleteSemaphore(ID semid)
		{
			int i;
			ID tmpid;
			Semaphore Semaphore;

			for (i = 0; i < m_SemaphoreTable.Count; i++) {
				Semaphore = m_SemaphoreTable[i];
				tmpid = Semaphore.SemID;

				if (semid == tmpid) {
					m_SemaphoreTable.RemoveAt(i);

					//delete Semaphore;

					return ER.E_OK;
				}
				else if (semid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public Semaphore GetSemaphore(ID semid)
		{
			int i;
			Semaphore Result;

			for (i = 0; i < m_SemaphoreTable.Count; i++) {
				Result = m_SemaphoreTable[i];
				if (semid == Result.SemID) {
					return Result;
				}
			}

			return null;
		}

		public ER GetCyclicHandler(HNO cycno, ref CyclicHandler Cyc)
		{
			if (cycno >= m_CyclicHandlerTable.Count)
				return ER.E_PAR;

			Cyc = m_CyclicHandlerTable[cycno];
			if (Cyc == null)
				return ER.E_NOEXS;

			return ER.E_OK;
		}

		public ER DefineCyclicHandler(HNO cycno, ref T_DCYC pk_dcyc)
		{
			CyclicHandler Cyc;

			//if (pk_dcyc == null)
			//	return ER.E_PAR;

			if (cycno >= m_CyclicHandlerTable.Count)
				return ER.E_PAR;

			Cyc = m_CyclicHandlerTable[cycno];

			// 登録の場合
			if (!pk_dcyc.cycdel) {
				if (pk_dcyc.cychdr == null)
					return ER.E_PAR;

				if (((uint)pk_dcyc.cycact & (~1u)) != 0)
					return ER.E_PAR;

				if (pk_dcyc.cyctim <= 0)
					return ER.E_PAR;

				if (Cyc != null) {
					Cyc.Redefine(ref pk_dcyc);
				}
				else {
					Cyc = new CyclicHandler(cycno, ref pk_dcyc, this);
					m_CyclicHandlerTable[cycno] = Cyc;
				}
			}
			// 登録破棄の場合
			else {
				if (Cyc != null) {
					//delete Cyc;
					m_CyclicHandlerTable[cycno] = null;
				}
			}

			return ER.E_OK;
		}

		public ER GetSystemTime(out SYSTIME pk_tim)
		{
			//if(pk_tim == null)
			//	return ER.E_PAR;

			pk_tim = m_SysTime;

			return ER.E_OK;
		}

		public ER SetSystemTime(ref SYSTIME pk_tim)
		{
			//if(pk_tim == null)
			//	return ER.E_PAR;

			m_SysTime = pk_tim;

			return ER.E_OK;
		}

		public ER ReferSystemStatus(ref T_RSYS pk_rsys)
		{
			//if(pk_rsys == null)
			//	return ER.E_PAR;

			m_rsys = pk_rsys;

			return ER.E_OK;
		}

		void LockTaskTable()
		{
			while (System.Threading.Interlocked.CompareExchange(ref m_Lock, 1, 0) == 1)
				System.Threading.Thread.Yield();
		}

		void UnlockTaskTable()
		{
			System.Threading.Interlocked.Exchange(ref m_Lock, 0);
		}

		public void ClearAllTask()
		{
			Task Task;
			CPUContext CPUContext;
			MemoryPool MemPool;
			MemoryPoolFixedsize MemPoolFx;
			Mailbox Mailbox;
			Semaphore Semaphore;
			CyclicHandler CyclicHandler;
			UdpCep UdpCep;
			TcpCep TcpCep;

			while (m_CyclicHandlerTable.Count != 0) {
				CyclicHandler = m_CyclicHandlerTable[0];
				m_CyclicHandlerTable.RemoveAt(0);
				//if(CyclicHandler != null)
				//	delete CyclicHandler;
			}

			do {
				LockTaskTable();
				try {
					for (int i = 0; i < m_TaskTable.Count; i++) {
						Task = m_TaskTable[i];
						CPUContext = (CPUContext)(Task.GetCPUContext());
						if ((CPUContext == null) || CPUContext.IsFinished()) {
							m_TaskTable.RemoveAt(i);
							i--;
							continue;
						}
						try {
							CPUContext.Terminate();
						}
						catch (System.Threading.ThreadAbortException) {
						}
						catch (Exception e) {
							System.Diagnostics.Debug.WriteLine(e.Message);
						}
					}
				}
				finally {
					UnlockTaskTable();
				}
				System.Threading.Thread.Yield();
			} while (m_TaskTable.Count != 0);

			while (m_MemPoolTable.Count != 0) {
				MemPool = m_MemPoolTable[0];
				MemPool.EnumMemoryBlock(this, EnumBlockCallBack);
				m_MemPoolTable.RemoveAt(0);
				//delete MemPool;
			}

			while (m_MemPoolFxTable.Count != 0) {
				MemPoolFx = m_MemPoolFxTable[0];
				m_MemPoolFxTable.RemoveAt(0);
				//delete MemPoolFx;
			}

			while (m_MailboxTable.Count != 0) {
				Mailbox = m_MailboxTable[0];
				m_MailboxTable.RemoveAt(0);
				//delete Mailbox;
			}

			while (m_SemaphoreTable.Count != 0) {
				Semaphore = m_SemaphoreTable[0];
				m_SemaphoreTable.RemoveAt(0);
				//delete Semaphore;
			}

			while (m_UdpCepTable.Count != 0) {
				UdpCep = m_UdpCepTable[0];
				m_UdpCepTable.RemoveAt(0);
				//delete UdpCep;
			}

			while (m_TcpCepTable.Count != 0) {
				TcpCep = m_TcpCepTable[0];
				m_TcpCepTable.RemoveAt(0);
				//delete TcpCep;
			}
		}

		private bool EnumBlockCallBack(object Obj, _CrtMemBlockHeader MemBlock)
		{
			throw new NotImplementedException();
		}

		internal void EnumMemoryBlock(MemoryPool mpl)
		{
			mpl.EnumMemoryBlock(this, EnumBlockCallBack);
		}

		internal ER CreateUdpCep(ID cepid, ref T_UDP_CCEP pk_ccep, out ID p_cepid)
		{
			int i;

			//if (pk_ccep == null)
			//	return ER.E_PAR;

			if (cepid == ID.ID_AUTO) {
				//if (p_cepid == 0)
				//	return ER.E_PAR;

				cepid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_UdpCepTable.Count) {
						UdpCep udpCep = new UdpCep(cepid, ref pk_ccep, this, m_lwIP);
						m_UdpCepTable.Add(udpCep);
						break;
					}

					if (cepid == m_UdpCepTable[i].CepID) {
						cepid.Value++;
					}
					else {
						UdpCep udpCep = new UdpCep(cepid, ref pk_ccep, this, m_lwIP);
						m_UdpCepTable.Insert(i, udpCep);
						break;
					}
				}
				p_cepid = cepid;
			}
			else {
				ID tmpid;

				p_cepid = ID.NULL;

				for (i = 0; i < m_UdpCepTable.Count; i++) {
					tmpid = m_UdpCepTable[i].CepID;

					if (cepid == tmpid) {
						return ER.E_OBJ;
					}
					else if (cepid < tmpid) {
						break;
					}
				}
				UdpCep udpCep = new UdpCep(cepid, ref pk_ccep, this, m_lwIP);
				m_UdpCepTable.Insert(i, udpCep);
			}

			return ER.E_OK;
		}

		public ER DeleteUdpCep(ID cepid)
		{
			int i;
			ID tmpid;
			UdpCep UdpCep;

			for (i = 0; i < m_UdpCepTable.Count; i++) {
				UdpCep = m_UdpCepTable[i];
				tmpid = UdpCep.CepID;

				if (cepid == tmpid) {
					m_UdpCepTable.RemoveAt(i);

					//delete UdpCep;

					return ER.E_OK;
				}
				else if (cepid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public UdpCep GetUdpCep(ID cepid)
		{
			int i;
			UdpCep Result;

			for (i = 0; i < m_UdpCepTable.Count; i++) {
				Result = m_UdpCepTable[i];
				if (cepid == Result.CepID) {
					return Result;
				}
			}

			return null;
		}

		internal ER CreateTcpCep(ID cepid, ref T_TCP_CCEP pk_ccep, out ID p_cepid)
		{
			int i;

			//if (pk_ccep == null)
			//	return ER.E_PAR;

			if (cepid == ID.ID_AUTO) {
				//if (p_cepid == 0)
				//	return ER.E_PAR;

				cepid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_TcpCepTable.Count) {
						TcpCep tcpCep = new TcpCep(cepid, ref pk_ccep, this, m_lwIP);
						m_TcpCepTable.Add(tcpCep);
						break;
					}

					if (cepid == m_TcpCepTable[i].CepID) {
						cepid.Value++;
					}
					else {
						TcpCep tcpCep = new TcpCep(cepid, ref pk_ccep, this, m_lwIP);
						m_TcpCepTable.Insert(i, tcpCep);
						break;
					}
				}
				p_cepid = cepid;
			}
			else {
				ID tmpid;

				p_cepid = ID.NULL;

				for (i = 0; i < m_TcpCepTable.Count; i++) {
					tmpid = m_TcpCepTable[i].CepID;

					if (cepid == tmpid) {
						return ER.E_OBJ;
					}
					else if (cepid < tmpid) {
						break;
					}
				}
				TcpCep tcpCep = new TcpCep(cepid, ref pk_ccep, this, m_lwIP);
				m_TcpCepTable.Insert(i, tcpCep);
			}

			return ER.E_OK;
		}

		public ER DeleteTcpCep(ID cepid)
		{
			int i;
			ID tmpid;
			TcpCep TcpCep;

			for (i = 0; i < m_TcpCepTable.Count; i++) {
				TcpCep = m_TcpCepTable[i];
				tmpid = TcpCep.CepID;

				if (cepid == tmpid) {
					m_TcpCepTable.RemoveAt(i);

					//delete TcpCep;

					return ER.E_OK;
				}
				else if (cepid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public TcpCep GetTcpCep(ID cepid)
		{
			int i;
			TcpCep Result;

			for (i = 0; i < m_TcpCepTable.Count; i++) {
				Result = m_TcpCepTable[i];
				if (cepid == Result.CepID) {
					return Result;
				}
			}

			return null;
		}

		internal ER CreateTcpRep(ID cepid, ref T_TCP_CREP pk_ccep, out ID p_cepid)
		{
			int i;

			//if (pk_ccep == null)
			//	return ER.E_PAR;

			if (cepid == ID.ID_AUTO) {
				//if (p_cepid == 0)
				//	return ER.E_PAR;

				cepid.Value = 1;

				for (i = 0; ; i++) {
					if (i >= m_TcpRepTable.Count) {
						TcpRep tcpRep = new TcpRep(cepid, ref pk_ccep, this, m_lwIP);
						m_TcpRepTable.Add(tcpRep);
						break;
					}

					if (cepid == m_TcpRepTable[i].RepID) {
						cepid.Value++;
					}
					else {
						TcpRep tcpRep = new TcpRep(cepid, ref pk_ccep, this, m_lwIP);
						m_TcpRepTable.Insert(i, tcpRep);
						break;
					}
				}
				p_cepid = cepid;
			}
			else {
				ID tmpid;

				p_cepid = ID.NULL;

				for (i = 0; i < m_TcpRepTable.Count; i++) {
					tmpid = m_TcpRepTable[i].RepID;

					if (cepid == tmpid) {
						return ER.E_OBJ;
					}
					else if (cepid < tmpid) {
						break;
					}
				}
				TcpRep tcpRep = new TcpRep(cepid, ref pk_ccep, this, m_lwIP);
				m_TcpRepTable.Insert(i, tcpRep);
			}

			return ER.E_OK;
		}

		public ER DeleteTcpRep(ID cepid)
		{
			int i;
			ID tmpid;
			TcpRep TcpRep;

			for (i = 0; i < m_TcpRepTable.Count; i++) {
				TcpRep = m_TcpRepTable[i];
				tmpid = TcpRep.RepID;

				if (cepid == tmpid) {
					m_TcpRepTable.RemoveAt(i);

					//delete TcpRep;

					return ER.E_OK;
				}
				else if (cepid < tmpid) {
					break;
				}
			}

			return ER.E_OBJ;
		}

		public TcpRep GetTcpRep(ID cepid)
		{
			int i;
			TcpRep Result;

			for (i = 0; i < m_TcpRepTable.Count; i++) {
				Result = m_TcpRepTable[i];
				if (cepid == Result.RepID) {
					return Result;
				}
			}

			return null;
		}
	}
}
