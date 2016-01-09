/**
 * @file
 * Stack-internal timers implementation.
 * This file includes timer callbacks for stack-internal timers as well as
 * functions to set up or stop timers and check for expired timers.
 *
 */

/*
 * Copyright (c) 2001-2004 Swedish Institute of Computer Science.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
 * IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 *
 * This file is part of the lwIP TCP/IP stack.
 *
 * Author: Adam Dunkels <adam@sics.se>
 *         Simon Goldschmidt
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	/** Function prototype for a timeout callback function. Register such a function
	 * using sys_timeout().
	 *
	 * @param arg Additional argument to pass to the function - set up by sys_timeout()
	 */
	public delegate void sys_timeout_handler(object arg);

	internal class sys_timeo : memp
	{
		public sys_timeo(lwip lwip) : base(lwip) { }

		public sys_timeo next;
		public int time;
		public sys_timeout_handler h;
		public object arg;
#if LWIP_DEBUG_TIMERNAMES
		public const pointer handler_name;
#endif // LWIP_DEBUG_TIMERNAMES
	}

	internal partial class sys
	{
#if LWIP_TIMERS
		/** The one and only timeout list */
		private static sys_timeo next_timeout;
#if NO_SYS
		public static uint timeouts_last_time;
#endif // NO_SYS

#if LWIP_TCP
		/** global variable that shows if the tcp timer is currently scheduled or not */
		private static int tcpip_tcp_timer_active;

		/**
		 * Timer callback function that calls tcp_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void tcpip_tcp_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);

			/* call TCP timer handler */
			lwip.tcp.tcp_tmr();
			/* timer still needed? */
			if (lwip.tcp.tcp_active_pcbs != null || lwip.tcp.tcp_tw_pcbs != null) {
				/* restart timer */
				sys_timeout(tcp.TCP_TMR_INTERVAL, tcpip_tcp_timer, null);
			}
			else {
				/* disable timer */
				tcpip_tcp_timer_active = 0;
			}
		}

		/**
		 * Called from TCP_REG when registering a new PCB:
		 * the reason is to have the TCP timer only running when
		 * there are active (or time-wait) PCBs.
		 */
		public void tcp_timer_needed()
		{
			/* timer is off but needed again? */
			if (tcpip_tcp_timer_active == 0 && (lwip.tcp.tcp_active_pcbs != null || lwip.tcp.tcp_tw_pcbs != null)) {
				/* enable and start timer */
				tcpip_tcp_timer_active = 1;
				sys_timeout(tcp.TCP_TMR_INTERVAL, tcpip_tcp_timer, null);
			}
		}
#endif // LWIP_TCP

#if IP_REASSEMBLY
		/**
		 * Timer callback function that calls ip_reass_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private static void
		ip_reass_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: ip_reass_tmr()\n");
			ip_reass_tmr();
			sys_timeout(IP_TMR_INTERVAL, ip_reass_timer, null);
		}
#endif // IP_REASSEMBLY

#if LWIP_ARP
		/**
		 * Timer callback function that calls etharp_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void arp_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: etharp_tmr()\n");
			lwip.etharp.etharp_tmr();
			sys_timeout(etharp_hdr.ARP_TMR_INTERVAL, arp_timer, null);
		}
#endif // LWIP_ARP

#if LWIP_DHCP
		/**
		 * Timer callback function that calls dhcp_coarse_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void dhcp_timer_coarse(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: dhcp_coarse_tmr()\n");
			lwip.dhcp.dhcp_coarse_tmr();
			sys_timeout(dhcp.DHCP_COARSE_TIMER_MSECS, dhcp_timer_coarse, null);
		}

		/**
		 * Timer callback function that calls dhcp_fine_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void dhcp_timer_fine(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: dhcp_fine_tmr()\n");
			lwip.dhcp.dhcp_fine_tmr();
			sys_timeout(dhcp.DHCP_FINE_TIMER_MSECS, dhcp_timer_fine, null);
		}
#endif // LWIP_DHCP

#if LWIP_AUTOIP
		/**
		 * Timer callback function that calls autoip_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private static void autoip_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: autoip_tmr()\n");
			autoip.autoip_tmr();
			sys_timeout(autoip.AUTOIP_TMR_INTERVAL, autoip_timer, null);
		}
#endif // LWIP_AUTOIP

#if LWIP_IGMP
		/**
		 * Timer callback function that calls igmp_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void igmp_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: igmp_tmr()\n");
			lwip.igmp.igmp_tmr();
			sys_timeout(igmp.IGMP_TMR_INTERVAL, igmp_timer, null);
		}
#endif // LWIP_IGMP

#if LWIP_DNS
		/**
		 * Timer callback function that calls dns_tmr() and reschedules itself.
		 *
		 * @param arg unused argument
		 */
		private void dns_timer(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "tcpip: dns_tmr()\n");
			lwip.dns.dns_tmr();
			sys_timeout(dns.DNS_TMR_INTERVAL, dns_timer, null);
		}
#endif // LWIP_DNS

		/** Initialize this module */
		public void sys_timeouts_init()
		{
#if IP_REASSEMBLY
			sys_timeout(IP_TMR_INTERVAL, ip_reass_timer, null);
#endif // IP_REASSEMBLY
#if LWIP_ARP
			sys_timeout(etharp_hdr.ARP_TMR_INTERVAL, arp_timer, null);
#endif // LWIP_ARP
#if LWIP_DHCP
			sys_timeout(dhcp.DHCP_COARSE_TIMER_MSECS, dhcp_timer_coarse, null);
			sys_timeout(dhcp.DHCP_FINE_TIMER_MSECS, dhcp_timer_fine, null);
#endif // LWIP_DHCP
#if LWIP_AUTOIP
			sys_timeout(autoip.AUTOIP_TMR_INTERVAL, autoip_timer, null);
#endif // LWIP_AUTOIP
#if LWIP_IGMP
			sys_timeout(igmp.IGMP_TMR_INTERVAL, igmp_timer, null);
#endif // LWIP_IGMP
#if LWIP_DNS
			sys_timeout(dns.DNS_TMR_INTERVAL, dns_timer, null);
#endif // LWIP_DNS

#if NO_SYS
			/* Initialise timestamp for sys_check_timeouts */
			timeouts_last_time = lwip.sys.sys_now();
#endif
		}

		/**
		 * Create a one-shot timer (aka timeout). Timeouts are processed in the
		 * following cases:
		 * - while waiting for a message using sys_timeouts_mbox_fetch()
		 * - by calling sys_check_timeouts() (NO_SYS==1 only)
		 *
		 * @param msecs time in milliseconds after that the timer should expire
		 * @param handler callback function to call when msecs have elapsed
		 * @param arg argument to pass to the callback function
		 */
#if LWIP_DEBUG_TIMERNAMES
		public void sys_timeout_debug(uint msecs, sys_timeout_handler handler, object arg, const pointer  handler_name)
#else // LWIP_DEBUG_TIMERNAMES
		public void sys_timeout(int msecs, sys_timeout_handler handler, object arg)
#endif // LWIP_DEBUG_TIMERNAMES
		{
			sys_timeo timeout, t;

			timeout = (sys_timeo)lwip.memp_malloc(memp_t.MEMP_SYS_TIMEOUT);
			if (timeout == null) {
				lwip.LWIP_ASSERT("sys_timeout: timeout != null, pool MEMP_SYS_TIMEOUT is empty", timeout != null);
				return;
			}
			timeout.next = null;
			timeout.h = handler;
			timeout.arg = arg;
			timeout.time = msecs;
#if LWIP_DEBUG_TIMERNAMES
			timeout.handler_name = handler_name;
			lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "sys_timeout: {0} msecs={1} handler={2} arg={3}\n",
				timeout, msecs, handler_name, arg));
#endif // LWIP_DEBUG_TIMERNAMES

			if (next_timeout == null) {
				next_timeout = timeout;
				return;
			}

			if (next_timeout.time > msecs) {
				next_timeout.time -= msecs;
				timeout.next = next_timeout;
				next_timeout = timeout;
			}
			else {
				for (t = next_timeout; t != null; t = t.next) {
					timeout.time -= t.time;
					if (t.next == null || t.next.time > timeout.time) {
						if (t.next != null) {
							t.next.time -= timeout.time;
						}
						timeout.next = t.next;
						t.next = timeout;
						break;
					}
				}
			}
		}

		/**
		 * Go through timeout list (for this task only) and remove the first matching
		 * entry, even though the timeout has not triggered yet.
		 *
		 * @note This function only works as expected if there is only one timeout
		 * calling 'handler' in the list of timeouts.
		 *
		 * @param handler callback function that would be called by the timeout
		 * @param arg callback argument that would be passed to handler
		*/
		public void sys_untimeout(sys_timeout_handler handler, object arg)
		{
			sys_timeo prev_t, t;

			if (next_timeout == null) {
				return;
			}

			for (t = next_timeout, prev_t = null; t != null; prev_t = t, t = t.next) {
				if ((t.h == handler) && (t.arg == arg)) {
					/* We have a match */
					/* Unlink from previous in list */
					if (prev_t == null) {
						next_timeout = t.next;
					}
					else {
						prev_t.next = t.next;
					}
					/* If not the last one, add time of this one back to next */
					if (t.next != null) {
						t.next.time += t.time;
					}
					lwip.memp_free(memp_t.MEMP_SYS_TIMEOUT, t);
					return;
				}
			}
			return;
		}

#if NO_SYS

		/** Handle timeouts for NO_SYS==1 (i.e. without using
		 * tcpip_thread/sys_timeouts_mbox_fetch(). Uses sys_now() to call timeout
		 * handler functions when timeouts expire.
		 *
		 * Must be called periodically from your main loop.
		 */
		public void sys_check_timeouts()
		{
			if (next_timeout != null) {
				sys_timeo tmptimeout;
				uint diff;
				sys_timeout_handler handler;
				object arg;
				byte had_one;
				uint now;

				now = sys_now();
				/* this cares for wraparounds */
				diff = now - timeouts_last_time;
				do {
#if PBUF_POOL_FREE_OOSEQ
					lwip.PBUF_CHECK_FREE_OOSEQ();
#endif // PBUF_POOL_FREE_OOSEQ
					had_one = 0;
					tmptimeout = next_timeout;
					if ((tmptimeout != null) && (tmptimeout.time <= diff)) {
						/* timeout has expired */
						had_one = 1;
						timeouts_last_time = now;
						diff = (uint)(diff - tmptimeout.time);
						next_timeout = tmptimeout.next;
						handler = tmptimeout.h;
						arg = tmptimeout.arg;
#if LWIP_DEBUG_TIMERNAMES
						if (handler != null) {
							lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "sct calling h={0} arg={1}\n",
								tmptimeout.handler_name, arg);
						}
#endif // LWIP_DEBUG_TIMERNAMES
						lwip.memp_free(memp_t.MEMP_SYS_TIMEOUT, tmptimeout);
						if (handler != null) {
							handler(arg);
						}
					}
					/* repeat until all expired timers have been called */
				} while (had_one != 0);
			}
		}

		/** Set back the timestamp of the last call to sys_check_timeouts()
		 * This is necessary if sys_check_timeouts() hasn't been called for a long
		 * time (e.g. while saving energy) to prevent all timer functions of that
		 * period being called.
		 */
		public void sys_restart_timeouts()
		{
			timeouts_last_time = lwip.sys.sys_now();
		}

#else // NO_SYS

		/**
		 * Wait (forever) for a message to arrive in an mbox.
		 * While waiting, timeouts are processed.
		 *
		 * @param mbox the mbox to fetch the message from
		 * @param msg the place to store the message
		 */
		public void sys_timeouts_mbox_fetch<P>(sys_mbox_t mbox, out P msg) where P : class
		{
			int time_needed;
			sys_timeo tmptimeout;
			sys_timeout_handler handler;
			object arg;

			msg = null;

		again:
			if (next_timeout == null)
			{
				time_needed = sys_arch_mbox_fetch(mbox, out msg, 0);
			}
			else
			{
				if (next_timeout.time > 0)
				{
					time_needed = sys_arch_mbox_fetch(mbox, out msg, next_timeout.time);
				}
				else
				{
					time_needed = sys.SYS_ARCH_TIMEOUT;
				}

				if (time_needed == sys.SYS_ARCH_TIMEOUT)
				{
					/* If time == sys.SYS_ARCH_TIMEOUT, a timeout occured before a message
					   could be fetched. We should now call the timeout handler and
					   deallocate the memory allocated for the timeout. */
					tmptimeout = next_timeout;
					next_timeout = tmptimeout.next;
					handler = tmptimeout.h;
					arg = tmptimeout.arg;
#if LWIP_DEBUG_TIMERNAMES
					if (handler != null) {
						lwip.LWIP_DEBUGF(opt.TIMERS_DEBUG, "stmf calling h={0} arg={1}\n",
							tmptimeout.handler_name, arg);
					}
#endif // LWIP_DEBUG_TIMERNAMES
					lwip.memp_free(memp_t.MEMP_SYS_TIMEOUT, tmptimeout);
					if (handler != null)
					{
						/* For LWIP_TCPIP_CORE_LOCKING, lock the core before calling the
						   timeout handler function. */
						lwip.tcpip.LOCK_TCPIP_CORE();
						handler(arg);
						lwip.tcpip.UNLOCK_TCPIP_CORE();
					}
					tcpip.LWIP_TCPIP_THREAD_ALIVE();

					/* We try again to fetch a message from the mbox. */
					goto again;
				}
				else
				{
					/* If time != sys.SYS_ARCH_TIMEOUT, a message was received before the timeout
					   occured. The time variable is set to the number of
					   milliseconds we waited for the message. */
					if (time_needed < next_timeout.time)
					{
						next_timeout.time -= time_needed;
					}
					else
					{
						next_timeout.time = 0;
					}
				}
			}
		}

#endif // NO_SYS

#else // LWIP_TIMERS
		/* Satisfy the TCP code which calls this function */
		public static void tcp_timer_needed()
		{
		}
#endif // LWIP_TIMERS
	}
}
