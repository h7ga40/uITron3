/**
 * @file
 * Statistics module
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
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct stats_proto
	{
		public uint xmit;             /* Transmitted packets. */
		public uint recv;             /* Received packets. */
		public uint fw;               /* Forwarded packets. */
		public uint drop;             /* Dropped packets. */
		public uint chkerr;           /* Checksum error. */
		public uint lenerr;           /* Invalid length error. */
		public uint memerr;           /* Out of memory error. */
		public uint rterr;            /* Routing error. */
		public uint proterr;          /* Protocol error. */
		public uint opterr;           /* Error in options. */
		public uint err;              /* Misc error. */
		public uint cachehit;
	};

	public struct stats_igmp
	{
		public uint xmit;             /* Transmitted packets. */
		public uint recv;             /* Received packets. */
		public uint drop;             /* Dropped packets. */
		public uint chkerr;           /* Checksum error. */
		public uint lenerr;           /* Invalid length error. */
		public uint memerr;           /* Out of memory error. */
		public uint proterr;          /* Protocol error. */
		public uint rx_v1;            /* Received v1 frames. */
		public uint rx_group;         /* Received group-specific queries. */
		public uint rx_general;       /* Received general queries. */
		public uint rx_report;        /* Received reports. */
		public uint tx_join;          /* Sent joins. */
		public uint tx_leave;         /* Sent leaves. */
		public uint tx_report;        /* Sent reports. */
	};

	public struct stats_mem
	{
#if LWIP_DEBUG
		public pointer name;
#endif // LWIP_DEBUG
		public uint avail;
		public uint used;
		public uint max;
		public uint err;
		public uint illegal;
	};

	public struct stats_syselem
	{
		public uint used;
		public uint max;
		public uint err;
	};

	public struct stats_sys
	{
		public stats_syselem sem;
		public stats_syselem mutex;
		public stats_syselem mbox;
	};

	public class lwip_stats
	{
#if true// LINK_STATS
		public stats_proto link;
#endif
#if true// ETHARP_STATS
		public stats_proto etharp;
#endif
#if true// IPFRAG_STATS
		public stats_proto ip_frag;
#endif
#if true// IP_STATS
		public stats_proto ip;
#endif
#if true// ICMP_STATS
		public stats_proto icmp;
#endif
#if true// IGMP_STATS
		public stats_igmp igmp;
#endif
#if true// UDP_STATS
		public stats_proto udp;
#endif
#if true// TCP_STATS
		public stats_proto tcp;
#endif
#if true// MEM_STATS
		public stats_mem mem;
#endif
#if true// MEMP_STATS
		public stats_mem[] memp = new stats_mem[(int)memp_t.MEMP_MAX];
#endif
#if true// SYS_STATS
		public stats_sys sys;
#endif
	};

	public partial class lwip
	{
		public lwip_stats lwip_stats = new lwip_stats();
	}

	public static class stats
	{
		public static void stats_init()
		{
#if LWIP_DEBUG
#if MEM_STATS
			lwip_stats.mem.name = "MEM";
#endif // MEM_STATS
#endif // LWIP_DEBUG
		}

#if SYS_STATS
#if LWIP_STATS_DISPLAY
		public static void stats_display_proto(stats_proto proto, pointer name)
		{
			LWIP_PLATFORM_DIAG(("\n{0}\n\t", name));
			LWIP_PLATFORM_DIAG(("xmit: {0}\n\t", proto.xmit)); 
			LWIP_PLATFORM_DIAG(("recv: {0}\n\t", proto.recv)); 
			LWIP_PLATFORM_DIAG(("fw: {0}\n\t", proto.fw)); 
			LWIP_PLATFORM_DIAG(("drop: {0}\n\t", proto.drop)); 
			LWIP_PLATFORM_DIAG(("chkerr: {0}\n\t", proto.chkerr)); 
			LWIP_PLATFORM_DIAG(("lenerr: {0}\n\t", proto.lenerr)); 
			LWIP_PLATFORM_DIAG(("memerr: {0}\n\t", proto.memerr)); 
			LWIP_PLATFORM_DIAG(("rterr: {0}\n\t", proto.rterr)); 
			LWIP_PLATFORM_DIAG(("proterr: {0}\n\t", proto.proterr)); 
			LWIP_PLATFORM_DIAG(("opterr: {0}\n\t", proto.opterr)); 
			LWIP_PLATFORM_DIAG(("err: {0}\n\t", proto.err)); 
			LWIP_PLATFORM_DIAG(("cachehit: {0}\n", proto.cachehit)); 
		}

#if IGMP_STATS
		public static void stats_display_igmp(stats_igmp igmp)
		{
			LWIP_PLATFORM_DIAG(("\nIGMP\n\t"));
			LWIP_PLATFORM_DIAG(("xmit: {0}\n\t", igmp.xmit)); 
			LWIP_PLATFORM_DIAG(("recv: {0}\n\t", igmp.recv)); 
			LWIP_PLATFORM_DIAG(("drop: {0}\n\t", igmp.drop)); 
			LWIP_PLATFORM_DIAG(("chkerr: {0}\n\t", igmp.chkerr)); 
			LWIP_PLATFORM_DIAG(("lenerr: {0}\n\t", igmp.lenerr)); 
			LWIP_PLATFORM_DIAG(("memerr: {0}\n\t", igmp.memerr)); 
			LWIP_PLATFORM_DIAG(("proterr: {0}\n\t", igmp.proterr)); 
			LWIP_PLATFORM_DIAG(("rx_v1: {0}\n\t", igmp.rx_v1)); 
			LWIP_PLATFORM_DIAG(("rx_group: {0}\n", igmp.rx_group));
			LWIP_PLATFORM_DIAG(("rx_general: {0}\n", igmp.rx_general));
			LWIP_PLATFORM_DIAG(("rx_report: {0}\n\t", igmp.rx_report)); 
			LWIP_PLATFORM_DIAG(("tx_join: {0}\n\t", igmp.tx_join)); 
			LWIP_PLATFORM_DIAG(("tx_leave: {0}\n\t", igmp.tx_leave)); 
			LWIP_PLATFORM_DIAG(("tx_report: {0}\n\t", igmp.tx_report)); 
		}
#endif // IGMP_STATS 

#if MEM_STATS || MEMP_STATS
		public static void stats_display_mem(stats_mem mem, pointer name)
		{
			LWIP_PLATFORM_DIAG(("\nMEM {0}\n\t", name));
			LWIP_PLATFORM_DIAG(("avail: {0}\n\t", (uint)mem.avail)); 
			LWIP_PLATFORM_DIAG(("used: {0}\n\t", (uint)mem.used)); 
			LWIP_PLATFORM_DIAG(("max: {0}\n\t", (uint)mem.max)); 
			LWIP_PLATFORM_DIAG(("err: {0}\n", (uint)mem.err));
		}
#endif // MEM_STATS || MEMP_STATS 

#if SYS_STATS
		public static void stats_display_sys(stats_sys sys)
		{
			LWIP_PLATFORM_DIAG(("\nSYS\n\t"));
			LWIP_PLATFORM_DIAG(("sem.used:  {0}\n\t", (uint)sys.sem.used)); 
			LWIP_PLATFORM_DIAG(("sem.max:   {0}\n\t", (uint)sys.sem.max)); 
			LWIP_PLATFORM_DIAG(("sem.err:   {0}\n\t", (uint)sys.sem.err)); 
			LWIP_PLATFORM_DIAG(("mutex.used: {0}\n\t", (uint)sys.mutex.used)); 
			LWIP_PLATFORM_DIAG(("mutex.max:  {0}\n\t", (uint)sys.mutex.max)); 
			LWIP_PLATFORM_DIAG(("mutex.err:  {0}\n\t", (uint)sys.mutex.err)); 
			LWIP_PLATFORM_DIAG(("mbox.used:  {0}\n\t", (uint)sys.mbox.used)); 
			LWIP_PLATFORM_DIAG(("mbox.max:   {0}\n\t", (uint)sys.mbox.max)); 
			LWIP_PLATFORM_DIAG(("mbox.err:   {0}\n\t", (uint)sys.mbox.err)); 
		}
#endif // SYS_STATS 

		public static void stats_display()
		{
			short i;

			LINK_STATS_DISPLAY();
			ETHARP_STATS_DISPLAY();
			IPFRAG_STATS_DISPLAY();
			IP_STATS_DISPLAY();
			IGMP_STATS_DISPLAY();
			ICMP_STATS_DISPLAY();
			UDP_STATS_DISPLAY();
			TCP_STATS_DISPLAY();
			MEM_STATS_DISPLAY();
			for (i = 0; i < memp_t.MEMP_MAX; i++) {
				MEMP_STATS_DISPLAY(i);
			}
			sys.SYS_STATS_DISPLAY();
		}
#endif // LWIP_STATS_DISPLAY
#endif // LWIP_STATS
#if false
		internal static void STATS_INC_USED(pointer x, int y)
		{
			lwip_stats[x].used += y;
			if (lwip_stats[x].max < lwip_stats[x].used)
			{
				lwip_stats[x].max = lwip_stats[x].used;
			}
		}
#endif
	}
}
