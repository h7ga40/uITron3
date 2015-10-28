using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uITron3
{
	public class sys
	{
		lwip lwip;
		static Random m_Random = new Random();
		public static int LWIP_RAND() { return m_Random.Next(); }

		public int old_level;
		public int lev;
		public const int SYS_MBOX_EMPTY = -1;

		public sys(lwip lwip)
		{
			this.lwip = lwip;
		}

		internal static uint sys_now()
		{
			return (uint)(DateTime.Now.Ticks / 10000);
		}

		public void SYS_ARCH_PROTECT(int lvl)
		{
		}

		public void SYS_ARCH_UNPROTECT(int lvl)
		{
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

		internal static void sys_init(lwip lwip)
		{
			lwip.sys = new sys(lwip);
		}

		internal static void tcp_timer_needed()
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
		internal static err_t tcpip_callback_with_block(Action<object> pbuf_free_ooseq_callback, object p, int v)
		{
			return err_t.ERR_OK;
		}
	}

	public class sys_timeo : memp
	{
		public sys_timeo(lwip lwip) : base(lwip) { }
	}
}
