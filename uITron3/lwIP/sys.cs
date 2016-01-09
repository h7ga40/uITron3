using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uITron3
{
	internal partial class sys
	{
		lwip lwip;
		Nucleus m_Nucleus;
		static Random m_Random = new Random();
		public static int LWIP_RAND() { return m_Random.Next(); }

		public int old_level;
		public int lev;
		public const int SYS_MBOX_EMPTY = -1;

		public sys(lwip lwip, Nucleus nucleus)
		{
			this.lwip = lwip;
			m_Nucleus = nucleus;
		}

		internal uint sys_now()
		{
			SYSTIME time = new SYSTIME();

			m_Nucleus.GetSystemTime(out time);

			return (uint)(time.Value);
		}

		public void SYS_ARCH_PROTECT(int lvl)
		{
			m_Nucleus.LockCPU();
		}

		public void SYS_ARCH_UNPROTECT(int lvl)
		{
			m_Nucleus.UnlockCPU();
		}

		public void SYS_ARCH_DECL_PROTECT(int lvl)
		{
		}

		internal err_t sys_mutex_new(sys_mutex_t mem_mutex)
		{
			return err_t.ERR_OK;
		}

		internal void sys_mutex_lock(sys_mutex_t mem_mutex)
		{
			Monitor.Enter(mem_mutex);
		}

		internal void sys_mutex_unlock(sys_mutex_t mem_mutex)
		{
			Monitor.Exit(mem_mutex);
		}

		internal static void sys_init(lwip lwip, Nucleus nucleus)
		{
			lwip.sys = new sys(lwip, nucleus);
		}

		internal void tcp_timer_needed()
		{

		}
	}

	partial class lwip
	{
		internal sys sys;
	}

	public class sys_mutex_t
	{

	}

	public class tcpip
	{
		public delegate void tcpip_callback_fn(object arg);

		public static err_t tcpip_callback_with_block(tcpip_callback_fn function, object arg, byte block)
		{
			return err_t.ERR_OK;
		}
	}
}
