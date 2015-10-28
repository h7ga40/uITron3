using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace uITron3
{
	internal delegate void TTaskExecute();

	internal class CPUContext : ICPUContext
	{
		Thread m_Thread;
		bool m_Terminate;
		bool m_Release;
		Kernel m_Kernel;
		Task m_Task;
		TTaskExecute m_Execute;
		long m_SuspendFlag;
		long m_DelaySuspend;
		bool m_Suspend;
		string m_Name;
		static ThreadLocal<CPUContext> m_TlsIndex;

		const long SUSPEND_FLAG_KERNEL_REQUEST = 1;
		const long SUSPEND_FLAG_POP_CONTEXT = 2;
		const long SUSPEND_FLAG_SWITCH_TO_KERNEL = 4;
		const long SUSPEND_FLAG_DELAY_SUSPEND = 8;
		const long SUSPEND_FLAG_END_DELAY = 16;
		const long SUSPEND_FLAG_SUSPENDING = 32;

		public CPUContext(Kernel pKernel)
		{
			m_Kernel = pKernel;
			m_Thread = null;
			m_Terminate = false;
			m_Release = false;
			m_Task = null;
			m_Execute = null;
			m_DelaySuspend = 0;
			m_SuspendFlag = 0;

			if (m_TlsIndex == null)
				m_TlsIndex = new ThreadLocal<CPUContext>();
		}

		public bool IsTerminated() { return m_Terminate || m_Kernel.IsTerminated(); }

		public bool IsReleased() { return m_Release; }

		public bool IsFinished()
		{
			return (m_Thread == null) || !m_Thread.IsAlive;
		}

		public bool IsCurrent() { return m_Thread == Thread.CurrentThread; }

		public Task GetTask() { return m_Task; }

		public void Terminate()
		{
			m_Terminate = true;
			PopContext();
		}

		public void Wait() { m_Thread.Join(); }

		private void ThreadProc()
		{
			m_TlsIndex.Value = this;

			if (!m_Terminate) {
				try {
					Suspend();

					m_Execute();
				}
				catch (Exception) {
				}
			}

			m_Kernel.ExitCPUContext(this);
		}

		public void Activate(Task Task, TTaskExecute pExecute)
		{
			if (m_Thread != null)
				throw new Exception();

			m_Task = Task;
			m_Execute = pExecute;
			m_SuspendFlag = SUSPEND_FLAG_KERNEL_REQUEST;

			m_Thread = new Thread(ThreadProc);
			if (m_Thread == null)
				throw new Exception();
			m_Thread.Start();

			while ((m_SuspendFlag & SUSPEND_FLAG_SWITCH_TO_KERNEL) == 0)
				Thread.Yield();
		}

		public bool Dispatch()
		{
			System.Diagnostics.Debug.Assert(IsCurrent());

			if (IsTerminated()) {
				return false;
			}

			m_Kernel.SwitchKernelMode(this);

			if (IsTerminated()) {
				return false;
			}

			return true;
		}

		public void PushContext()
		{
			System.Diagnostics.Debug.Assert(m_Kernel.InKernelMode());

			long count = Interlocked.Add(ref m_SuspendFlag, SUSPEND_FLAG_KERNEL_REQUEST) - SUSPEND_FLAG_KERNEL_REQUEST;

			System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_KERNEL_REQUEST) == 0);

			if (count == SUSPEND_FLAG_DELAY_SUSPEND) {
				for (; ; ) {
					switch (m_SuspendFlag & (SUSPEND_FLAG_DELAY_SUSPEND | SUSPEND_FLAG_END_DELAY | SUSPEND_FLAG_SUSPENDING)) {
					case 0:
						m_Thread.Suspend();
						break;
					case SUSPEND_FLAG_END_DELAY:
						continue;
					case SUSPEND_FLAG_SUSPENDING:
					default:
						break;
					}
					break;
				}
			}
		}

		public void PopContext()
		{
			System.Diagnostics.Debug.Assert(m_Kernel.InKernelMode());

			long count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_KERNEL_REQUEST + SUSPEND_FLAG_POP_CONTEXT) + SUSPEND_FLAG_KERNEL_REQUEST - SUSPEND_FLAG_POP_CONTEXT;

			System.Diagnostics.Debug.Assert((count & (SUSPEND_FLAG_KERNEL_REQUEST | SUSPEND_FLAG_POP_CONTEXT)) == SUSPEND_FLAG_KERNEL_REQUEST);

			if ((count & SUSPEND_FLAG_SWITCH_TO_KERNEL) != 0)
				m_Suspend = false;

			if ((m_Thread.ThreadState & (ThreadState.SuspendRequested | ThreadState.Suspended)) != 0)
				m_Thread.Resume();

			count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_POP_CONTEXT) + SUSPEND_FLAG_POP_CONTEXT;
		}

		public void Suspend()
		{
			System.Diagnostics.Debug.Assert(IsCurrent());

			m_Suspend = true;

			long count = Interlocked.Add(ref m_SuspendFlag, SUSPEND_FLAG_SWITCH_TO_KERNEL) - SUSPEND_FLAG_SWITCH_TO_KERNEL;

			System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_SWITCH_TO_KERNEL) == 0x0);

			while (m_Suspend && !IsTerminated()) {
				m_Thread.Suspend();
			}

			count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_SWITCH_TO_KERNEL) + SUSPEND_FLAG_SWITCH_TO_KERNEL;

			System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_SWITCH_TO_KERNEL) != 0x0);
		}

		public void StartDelaySuspend()
		{
			System.Diagnostics.Debug.Assert(IsCurrent());

			m_DelaySuspend++;

			if (m_DelaySuspend == 1) {
				long count = Interlocked.Add(ref m_SuspendFlag, SUSPEND_FLAG_DELAY_SUSPEND) - SUSPEND_FLAG_DELAY_SUSPEND;

				System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_DELAY_SUSPEND) == 0);
			}
		}

		public bool EndDelaySuspend()
		{
			System.Diagnostics.Debug.Assert(IsCurrent());
			System.Diagnostics.Debug.Assert(m_DelaySuspend > 0);

			m_DelaySuspend--;

			if (m_DelaySuspend == 0) {
				long count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_DELAY_SUSPEND + SUSPEND_FLAG_END_DELAY) + SUSPEND_FLAG_DELAY_SUSPEND - SUSPEND_FLAG_END_DELAY;

				System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_DELAY_SUSPEND) != 0);

				if (m_SuspendFlag == (SUSPEND_FLAG_KERNEL_REQUEST | SUSPEND_FLAG_END_DELAY)) {
					count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_END_DELAY + SUSPEND_FLAG_SUSPENDING) + SUSPEND_FLAG_END_DELAY - SUSPEND_FLAG_SUSPENDING;

					System.Diagnostics.Debug.Assert((count & (SUSPEND_FLAG_END_DELAY | SUSPEND_FLAG_SUSPENDING)) == SUSPEND_FLAG_END_DELAY);

					m_Thread.Suspend();

					count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_SUSPENDING) + SUSPEND_FLAG_SUSPENDING;

					System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_SUSPENDING) != 0);
				}
				else {
					count = Interlocked.Add(ref m_SuspendFlag, -SUSPEND_FLAG_END_DELAY) + SUSPEND_FLAG_END_DELAY;

					System.Diagnostics.Debug.Assert((count & SUSPEND_FLAG_END_DELAY) != 0);
				}
			}

			return true;
		}

		public void Release()
		{
			m_Release = true;
		}

		public void SetThreadName(string szThreadName)
		{
			m_Name = szThreadName;
			m_Thread.Name = szThreadName;
		}

		public static CPUContext GetCurrent()
		{
			if (m_TlsIndex == null)
				return null;
			return m_TlsIndex.Value;
		}

		public void ClearTask()
		{
			m_Task = null;
		}
	}
}
