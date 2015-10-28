using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace uITron3
{
	public delegate void TKernelEvent();
	public delegate void TOutputEvent(int kind, byte[] data);
	public delegate void TGetSystemTimeEvent(ref long now, ref long frequency);

	public struct TCallbackEvent
	{
		public int Func;
		public int Kind;
		public byte[] Data;
	}

	public abstract class Kernel : IKernel
	{
		Thread m_Thread;
		bool m_Terminate;
		ManualResetEvent m_IntEvent;
		System.Threading.Semaphore m_SysSem;
		ThreadLocal<int> m_TlsIndex;
		long m_Locked;
		bool m_TaskMode;
		long m_Frequency;
		Nucleus m_Nucleus;
		ICPUContext m_Current;
		TKernelEvent m_OnSetEvent;
		TKernelEvent m_OnStart;
		TKernelEvent m_OnTerminate;
		TKernelEvent m_OnIdle;
		TOutputEvent m_OnOutput;
		TGetSystemTimeEvent m_OnGetSystemTimeEvent;
		LinkedList<TSystemIFItem> m_SystemIFList = new LinkedList<TSystemIFItem>();
		string m_UnitName = "";
		long m_Lock;
		protected T_DINT[] m_IntHandler;
		protected bool[] m_InProcIntr;
		System.Threading.Semaphore m_CallbackSem;
		LinkedList<TCallbackEvent> m_EventQueue = new LinkedList<TCallbackEvent>();

		public Kernel(long frequency, int sysTmrIntNo, TMO sysTmrIntv)
		{
			m_Nucleus = new Nucleus(this, sysTmrIntNo, sysTmrIntv);

			m_Frequency = frequency;
			m_OnSetEvent = null;
			m_OnStart = null;
			m_OnTerminate = null;
			m_OnIdle = null;
			m_OnOutput = null;
			m_OnGetSystemTimeEvent = null;

			m_Thread = null;
			m_Terminate = false;
			m_IntEvent = null;
			m_Locked = 0;
			m_TaskMode = false;
			m_TlsIndex = new ThreadLocal<int>();
			m_SysSem = new System.Threading.Semaphore(1, 1);
			m_CallbackSem = new System.Threading.Semaphore(1, 1);
			m_Lock = 0;
		}

		public bool IsTerminated() { return m_Terminate; }

		public bool IsAlibe
		{
			get { return (m_Thread != null) && m_Thread.IsAlive; }
		}

		public bool IsFinished()
		{
			return (m_Thread == null) || !m_Thread.IsAlive;
		}

		public bool InKernelMode() { return m_Thread == Thread.CurrentThread; }

		internal Nucleus Nucleus { get { return m_Nucleus; } }

		public TKernelEvent OnSetEvent { get { return m_OnSetEvent; } set { m_OnSetEvent = value; } }
		public TKernelEvent OnStart { get { return m_OnStart; } set { m_OnStart = value; } }
		public TKernelEvent OnTerminate { get { return m_OnTerminate; } set { m_OnTerminate = value; } }
		public TKernelEvent OnIdle { get { return m_OnIdle; } set { m_OnIdle = value; } }
		public TOutputEvent OnOutput { get { return m_OnOutput; } set { m_OnOutput = value; } }
		public TGetSystemTimeEvent OnGetSystemTime { get { return m_OnGetSystemTimeEvent; } }

		ICPUContext IKernel.GetCurrent() { return /*m_Current*/CPUContext.GetCurrent(); }

		protected void SpinLock()
		{
			while (Interlocked.CompareExchange(ref m_Lock, 1, 0) == 1) Thread.Yield();
		}

		protected void SpinUnlock()
		{
			Interlocked.Exchange(ref m_Lock, 0);
		}

		public void Start(bool CreateSuspend)
		{
			if (m_Thread != null)
				throw new Exception();

			m_Thread = new Thread(ThreadProc);

			m_IntEvent = new ManualResetEvent(false);
			if (m_IntEvent == null) {
				m_Thread = null;
				throw new Exception();
			}

			m_Thread.Name = "Kernel";
			m_Thread.Start();
		}

		public void Terminate()
		{
			if (m_Terminate && m_Thread == null) {
				DoTerminate();
			}
			else {
				m_Terminate = true;
				m_IntEvent.Set();
			}
			if ((m_Current != null) && m_Current.IsCurrent()) {
				ExitCPUContext(m_Current);
			}
		}

		void ThreadProc()
		{
			if (!m_Terminate) {
				try {
					Execute();
				}
				catch (Exception) {
				}
			}

			m_Thread = null;
		}

		void DoSetEvent()
		{
			if (m_OnSetEvent != null)
				m_OnSetEvent();
		}

		void DoStart()
		{
			TCallbackEvent callback = new TCallbackEvent();

			callback.Func = 0;

			m_CallbackSem.WaitOne();

			try {
				m_EventQueue.AddLast(callback);
			}
			finally {
				m_CallbackSem.Release();
			}

			DoSetEvent();
		}

		void DoTerminate()
		{
			TCallbackEvent callback = new TCallbackEvent();

			callback.Func = 1;

			if (!m_CallbackSem.WaitOne())
				throw new Exception();

			try {
				m_EventQueue.AddLast(callback);
			}
			finally {
				m_CallbackSem.Release();
			}

			DoSetEvent();
		}

		void Execute()
		{
			try {
				LockCPU();
				try {
					DoStart();

					MainLoop();
				}
				catch (Exception e) {
					System.Diagnostics.Debug.WriteLine(e.Message);
				}
				finally {
					UnlockCPU();
				}

				m_Nucleus.ClearAllTask();
			}
			finally {
				DoTerminate();
			}
		}

		public abstract void Init(Itron itron);
		protected abstract void Start();
		protected abstract bool InterruptEnabled(int intNo);
		protected abstract void SetInterrupt(int intNo);
		protected abstract void ClearInterrupt(int intNo);
		public abstract ER DefineInterruptHandler(uint dintno, ref T_DINT pk_dint);
		public abstract ER ChangeInterruptControlRegister(uint dintno, byte icrcmd);
		public abstract void Input(int kind, byte[] data);
		public abstract TMO GetTimer();
		public abstract void Progress(TMO interval);
		public abstract void CallTimeOut();

		public bool InProcIntr(int intNo) { return m_InProcIntr[intNo]; }

		void ProcInterrupt()
		{
			// 割り込みの受付を完了
			for (int intNo = 0; intNo < m_InProcIntr.Length; intNo++) {
				lock (m_InProcIntr) {
					bool prev = m_InProcIntr[intNo];
					m_InProcIntr[intNo] = false;
					if (prev)
						SetInterrupt(intNo);
				}
			}
		}

		void MainLoop()
		{
			int intNo;
			ID tskid;
			Task task;
			bool noIntr;

			Start();

			m_Nucleus.Start();

			do {
				ProcInterrupt();

				do {
					noIntr = true;
					for (intNo = 0; intNo < m_InProcIntr.Length; intNo++) {
						if (IsTerminated())
							return;

						if (InterruptEnabled(intNo)) {
							noIntr = false;

							ClearInterrupt(intNo);

							if (intNo == Nucleus.SysTmrIntNo) {
								m_Nucleus.OnSysTime();
							}

							tskid = CallIntHandler(intNo);

							if (tskid != ID.TSK_NULL) {
								Task wTask;

								wTask = Nucleus.GetTask(tskid);
								if (wTask != null) {
									wTask.Wakeup();
								}
							}
						}
					}
				}
				while (!noIntr);

				m_Nucleus.Scheduling();

				task = m_Nucleus.GetScheduledTask();
				if (task == null) {
					Idle();
				}
				else {
					m_Current = (CPUContext)task.GetCPUContext();

					task.Run();

					m_Current.PopContext();

					UnlockCPU();

					m_TaskMode = true;

					if (!m_IntEvent.WaitOne()) {
						Terminate();
					}

					m_TaskMode = false;

					LockCPU();

					CPUContext CPUContext = (CPUContext)m_Current;
					m_Current = null;

					if (CPUContext.IsReleased()) {
						CPUContext.Wait();
						CPUContext.ClearTask();
					}
					else
						CPUContext.PushContext();
				}

				task = m_Nucleus.GetCurrentTask();
				if (task != null) {
					task.Ready();
				}

			} while (!IsTerminated());
		}

		void Idle()
		{
			ID tskid;

			tskid = DoIdle();

			if (tskid != ID.TSK_NULL) {
				Task Task;

				Task = Nucleus.GetTask(tskid);
				if (Task != null) {
					Task.Wakeup();

					m_Nucleus.Scheduling();
				}
			}
			else {
				UnlockCPU();

				if (!m_IntEvent.WaitOne()) {
					Terminate();
				}

				LockCPU();
			}
		}

		bool IKernel.Dispatch()
		{
			bool Result;
			Task Task;
			ICPUContext CPUContext;

			Task = Nucleus.GetTask(ID.TSK_SELF);

			if ((Task != null) && (Task.rtsk.tskstat != TSKSTAT.TTS_DMT)) {
				CPUContext = Task.GetCPUContext();

				Task.Ready();

				Result = CPUContext.Dispatch();
			}
			else {
				m_IntEvent.Set();
				Result = !IsTerminated();
			}

			return Result;
		}

		bool IKernel.ExitAndDispatch()
		{
			ExitCPUContext(CPUContext.GetCurrent());
			return true;
		}

		ID DoIdle()
		{
			ID tskid = ID.TSK_NULL;

			TCallbackEvent callback = new TCallbackEvent();

			callback.Func = 2;

			if (!m_CallbackSem.WaitOne())
				throw new Exception();

			try {
				m_EventQueue.AddLast(callback);
			}
			finally {
				m_CallbackSem.Release();
			}

			DoSetEvent();

			return tskid;
		}

		ID CallIntHandler(int intNo)
		{
			ID tskid = ID.TSK_NULL;

			if (m_IntHandler[intNo].inthdr != null) {
				try {
					tskid = m_IntHandler[intNo].inthdr(m_IntHandler[intNo].gp);
				}
				catch (Exception) {
				}
			}

			return tskid;
		}

		ICPUContext IKernel.NewCPUContext()
		{
			return new CPUContext(this);
		}

		internal void ExitCPUContext(ICPUContext CPUContext)
		{
			System.Diagnostics.Debug.Assert(CPUContext.IsCurrent());

			((CPUContext)CPUContext).Release();

			// UnlockCPUでは、deleteしたCPUContextに対してEndDelaySuspendを
			// 呼んでしまうので、その手前の処理のみ実行。
			int TlsLockCount = (int)m_TlsIndex.Value;
			TlsLockCount--;

			// ロック解除
			if (TlsLockCount == 0) {
				m_SysSem.Release();
				Interlocked.Decrement(ref m_Locked);
			}

			m_TlsIndex.Value = TlsLockCount;

			m_IntEvent.Set();

			m_Thread.Abort();
		}

		public void LockCPU()
		{
			int TlsLockCount = m_TlsIndex.Value;

			// 他のスレッドが動かないようロック
			if (TlsLockCount == 0) {
				Interlocked.Increment(ref m_Locked);
				for (; ; ) {
					if (!m_SysSem.WaitOne()) {
						Terminate();
						break;
					}
					// 実行を意図したスレッドかチェック
					CPUContext Context = CPUContext.GetCurrent();
					if ((Context == null) || (Context == m_Current) || (m_Current == null))
						break;
					// 実行したくないスレッドはもう一度待つ
					m_SysSem.Release();
					Thread.Yield();
				}
			}

			TlsLockCount++;
			m_TlsIndex.Value = TlsLockCount;

			if (TlsLockCount == 1) {
				CPUContext LockCPUContext = CPUContext.GetCurrent();
				if (LockCPUContext != null)
					LockCPUContext.StartDelaySuspend();
			}
		}

		public void UnlockCPU()
		{
			int TlsLockCount = m_TlsIndex.Value;
			TlsLockCount--;

			System.Diagnostics.Debug.Assert(TlsLockCount >= 0);

			// ロック解除
			if (TlsLockCount == 0) {
				m_SysSem.Release();
				Interlocked.Decrement(ref m_Locked);
			}

			m_TlsIndex.Value = TlsLockCount;

			if (TlsLockCount == 0) {
				CPUContext LockCPUContext = CPUContext.GetCurrent();
				if (LockCPUContext != null)
					LockCPUContext.EndDelaySuspend();
			}
		}

		internal void SwitchKernelMode(CPUContext CPUContext)
		{
			int TlsLockCount;

			TlsLockCount = m_TlsIndex.Value;

			System.Diagnostics.Debug.Assert(TlsLockCount > 0);

			while ((m_Locked == 0) && !m_TaskMode)
				Thread.Yield();

			// ロック解除
			m_SysSem.Release();
			Interlocked.Add(ref m_Locked, -TlsLockCount);

			m_TlsIndex.Value = 0;

			m_IntEvent.Set();

			CPUContext.Suspend();

			// 他のスレッドが動かないようロック
			Interlocked.Add(ref m_Locked, TlsLockCount);
			if (!m_SysSem.WaitOne()) {
				Terminate();
			}

			m_TlsIndex.Value = TlsLockCount;
		}

		public void Interrupt(int intNo)
		{
			if (!IsTerminated() && (intNo >= 0) && (intNo < m_InProcIntr.Length)) {
				lock (m_InProcIntr) {
					bool prev = m_InProcIntr[intNo];
					m_InProcIntr[intNo] = true;
					if (!prev)
						m_IntEvent.Set();
				}
			}
		}

		CPUContext StartDelaySuspend()
		{
			CPUContext CPUContext = CPUContext.GetCurrent();

			if (CPUContext != null) {
				CPUContext.StartDelaySuspend();
			}

			return CPUContext;
		}

		void EndDelaySuspend(CPUContext CPUContext)
		{
			if (CPUContext == null)
				return;

			if (!CPUContext.EndDelaySuspend()) {
				CPUContext.Terminate();
			}
		}

		protected ER DefineSysIF(uint Addr, int Size, uint Substitute, ISystemIF SystemIF)
		{
			TSystemIFItem Item = new TSystemIFItem();

			Item.Addr = Addr;
			Item.Size = Size;
			Item.Substitute = Substitute;
			Item.SystemIF = SystemIF;

			m_SystemIFList.AddLast(Item);

			return ER.E_OK;
		}

		public byte GetSubByte(uint SubAddr)
		{
			byte Result = 0;
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Result = Item.SystemIF.GetByte(SubAddr);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();

			return Result;
		}

		public void SetSubByte(uint SubAddr, byte Value)
		{
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Item.SystemIF.SetByte(SubAddr, Value);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();
		}

		public ushort GetSubUInt16(uint SubAddr)
		{
			ushort Result = 0;
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Result = Item.SystemIF.GetUInt16(SubAddr);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();

			return Result;
		}

		public void SetSubUInt16(uint SubAddr, ushort Value)
		{
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Item.SystemIF.SetUInt16(SubAddr, Value);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();
		}

		public uint GetSubUInt32(uint SubAddr)
		{
			uint Result = 0;
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Result = Item.SystemIF.GetUInt32(SubAddr);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();

			return Result;
		}

		public void SetSubUInt32(uint SubAddr, uint Value)
		{
			uint Addr;
			bool OK = false;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				foreach (TSystemIFItem Item in m_SystemIFList) {
					Addr = SubAddr - (uint)Item.Substitute + Item.Addr;

					if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
						Item.SystemIF.SetUInt32(SubAddr, Value);
						OK = true;
						break;
					}
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			if (!OK)
				throw new Exception();
		}

		public byte GetByte(uint Addr)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					return Item.SystemIF.GetByte((uint)Item.Substitute + Addr - Item.Addr);
				}
			}

			return 0xFF;
		}

		public void SetByte(uint Addr, byte Value)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					Item.SystemIF.SetByte((uint)Item.Substitute + Addr - Item.Addr, Value);
					return;
				}
			}
		}

		public ushort GetUInt16(uint Addr)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					return Item.SystemIF.GetUInt16((uint)Item.Substitute + Addr - Item.Addr);
				}
			}

			return 0xFFFF;
		}

		public void SetUInt16(uint Addr, ushort Value)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					Item.SystemIF.SetUInt16((uint)Item.Substitute + Addr - Item.Addr, Value);
					return;
				}
			}
		}

		public uint GetUInt32(uint Addr)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					return Item.SystemIF.GetUInt32((uint)Item.Substitute + Addr - Item.Addr);
				}
			}

			return 0xFFFFFFFF;
		}

		public void SetUInt32(uint Addr, uint Value)
		{
			foreach (TSystemIFItem Item in m_SystemIFList) {
				if (((int)Addr >= Item.Addr) && ((int)Addr < Item.Addr + Item.Size)) {
					Item.SystemIF.SetUInt32((uint)Item.Substitute + Addr - Item.Addr, Value);
					return;
				}
			}
		}

		public void Output(int Kind, byte[] Data, int Size)
		{
			if (IsFinished())
				return;

			CPUContext CPUContext = StartDelaySuspend();
			try {
				TCallbackEvent callback = new TCallbackEvent();

				callback.Func = 3;
				callback.Kind = Kind;
				callback.Data = new byte[Size];
				Buffer.BlockCopy(Data, 0, callback.Data, 0, Size);

				if (!m_CallbackSem.WaitOne())
					throw new Exception();

				try {
					m_EventQueue.AddLast(callback);
				}
				finally {
					m_CallbackSem.Release();
				}

				DoSetEvent();
			}
			finally {
				EndDelaySuspend(CPUContext);
			}
		}

		public void GetSystemTime(ref long now, ref long frequency)
		{
			if (m_OnGetSystemTimeEvent != null)
				m_OnGetSystemTimeEvent(ref now, ref frequency);
		}

		public void Log(string Text)
		{
			Kernel g_Kernel = this;
			byte[] text = Encoding.UTF8.GetBytes(Text);
			g_Kernel.Output(-1, text, text.Length);
		}

		public string GetFullPathName(string Extension)
		{
			string Buffer;
			string drive;
			string fname;

			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			Buffer = asm.Location;
			drive = System.IO.Path.GetDirectoryName(Buffer);
			fname = System.IO.Path.GetFileNameWithoutExtension(Buffer);
			fname.Replace(".vshost", "");

			return System.IO.Path.Combine(drive, fname + Extension);
		}

		public int ReadFile(string Ext, int Pos, byte[] Data, int Size)
		{
			int result = 0;
			CPUContext CPUContext = StartDelaySuspend();
			try {
				string FileName;

				FileName = GetFullPathName(Ext);
				using (System.IO.FileStream File = new System.IO.FileStream(FileName, System.IO.FileMode.Open)) {
					File.Seek(Pos, System.IO.SeekOrigin.Begin);
					result = File.Read(Data, 0, Size);
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			return result;
		}

		public int WriteFile(string Ext, int Pos, byte[] Data, int Size)
		{
			int result = 0;
			CPUContext CPUContext = StartDelaySuspend();
			try {
				string FileName;

				FileName = GetFullPathName(Ext);
				using (System.IO.FileStream File = new System.IO.FileStream(FileName, System.IO.FileMode.OpenOrCreate)) {
					File.Seek(Pos, System.IO.SeekOrigin.Begin);
					File.Write(Data, 0, Size);
					result = (int)(File.Position - Pos);
				}
			}
			finally {
				EndDelaySuspend(CPUContext);
			}

			return result;
		}

		public bool ProcessEvent()
		{
			TCallbackEvent callback;

			while (m_EventQueue.Count > 0) {
				if (!m_CallbackSem.WaitOne())
					return false;

				try {
					callback = m_EventQueue.First.Value;
					m_EventQueue.RemoveFirst();
				}
				finally {
					m_CallbackSem.Release();
				}

				switch (callback.Func) {
				case 0:
					if (m_OnStart != null) {
						m_OnStart();
					}
					break;
				case 1:
					if (m_OnTerminate != null) {
						m_OnTerminate();
					}
					return false;
				case 2:
					if (m_OnIdle != null) {
						m_OnIdle();
					}
					break;
				case 3:
					if (m_OnOutput != null) {
						m_OnOutput(callback.Kind, callback.Data);
					}
					break;
				}
			}

			return !IsTerminated();
		}

		public void SetTaskName(ID tskid, string szName)
		{
			Kernel g_Kernel = this;
			CPUContext CPUContext = g_Kernel.StartDelaySuspend();
			try {
				string Name = g_Kernel.m_UnitName + "." + szName;

				Task task = g_Kernel.Nucleus.GetTask(tskid);

				CPUContext tc = (CPUContext)(task.GetCPUContext());

				tc.SetThreadName(Name);
			}
			finally {
				g_Kernel.EndDelaySuspend(CPUContext);
			}
		}

		protected void SetBit(int bit, uint addr)
		{
			SetByte(addr, (byte)(GetByte(addr) | (1u << (bit & 7))));
		}

		protected void ClearBit(int bit, uint addr)
		{
			SetByte(addr, (byte)(GetByte(addr) & (~(1u << (bit & 7)))));
		}

		protected bool TestBit(int bit, uint addr)
		{
			return (GetByte(addr) & (1u << (bit & 7))) != 0;
		}

		public int ReadAddr(uint addr, byte[] dst, int pos, int count)
		{
			int di;
			int ds;
			int dc;
			int i, n = count;

			// ２バイト境界へアライメント調整
			dc = pos;
			if ((addr & 0x01u) != 0) {
				dst[dc++] = GetByte(addr);
				addr++;
				n--;
			}

			if (n > 1) {
				// ４バイト境界へアライメント調整
				ds = dc;
				if ((addr & 0x02u) != 0) {
					Buffer.BlockCopy(BitConverter.GetBytes(GetUInt16(addr)), 0, dst, ds, 2);
					ds += 2;
					addr += 2;
					n -= 2;
				}

				di = ds;
				i = n >> 2;
				n -= i << 2;

				// ４バイト単位でデータバイト数分繰り返し
				while (--i >= 0) {
					// ４バイトコピー
					Buffer.BlockCopy(BitConverter.GetBytes(GetUInt32(addr)), 0, dst, di, 4);
					di += 4;
					addr += 4;
				}

				ds = di;
				i = n >> 1;
				n -= i << 1;

				// ２バイト単位でデータバイト数分繰り返し
				while (--i >= 0) {
					// ２バイトコピー
					Buffer.BlockCopy(BitConverter.GetBytes(GetUInt16(addr)), 0, dst, ds, 2);
					ds += 2;
					addr += 2;
				}

				dc = ds;
			}

			// 残りのデータバイト数分繰り返し
			while (--n >= 0) {
				// １バイトコピー
				dst[dc++] = GetByte(addr);
				addr++;
			}

			return count;
		}

		public int WriteAddr(uint Addr, byte[] src, int pos, int count)
		{
			int di;
			int ds;
			int dc;
			int i, n = count;

			// ２バイト境界へアライメント調整
			dc = pos;
			if ((Addr & 0x01u) != 0) {
				SetByte(Addr, src[dc++]);
				Addr++;
				n--;
			}

			if (n > 1) {
				// ４バイト境界へアライメント調整
				ds = dc;
				if ((Addr & 0x02u) != 0) {
					SetUInt16(Addr, BitConverter.ToUInt16(src, ds));
					ds += 2;
					Addr += 2;
					n -= 2;
				}

				di = ds;
				i = n >> 2;
				n -= i << 2;

				// ４バイト単位でデータバイト数分繰り返し
				while (--i >= 0) {
					// ４バイトコピー
					SetUInt32(Addr, BitConverter.ToUInt32(src, di));
					di += 4;
					Addr += 4;
				}

				ds = di;
				i = n >> 1;
				n -= i << 1;

				// ２バイト単位でデータバイト数分繰り返し
				while (--i >= 0) {
					// ２バイトコピー
					SetUInt16(Addr, BitConverter.ToUInt16(src, ds));
					ds += 2;
					Addr += 2;
				}

				dc = ds;
			}

			// 残りのデータバイト数分繰り返し
			while (--n >= 0) {
				// １バイトコピー
				SetByte(Addr, src[dc++]);
				Addr++;
			}

			return count;
		}
	}
}
