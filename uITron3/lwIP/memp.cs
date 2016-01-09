/**
 * @file
 * Dynamic pool memory manager
 *
 * lwIP has dedicated pools for many structures (netconn, protocol control blocks,
 * packet buffers, ...). All these pools are managed here.
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
	/* Create the list of all memory pools managed by memp. MEMP_MAX represents a NULL pool at the end */
	public enum memp_t
	{
		MEMP_RAW_PCB = 1,
		MEMP_UDP_PCB,
		MEMP_TCP_PCB,
		MEMP_TCP_PCB_LISTEN,
		MEMP_TCP_SEG,
		MEMP_REASSDATA,
		MEMP_FRAG_PBUF,
		MEMP_NETBUF,
		MEMP_NETCONN,
		MEMP_TCPIP_MSG_API,
		MEMP_TCPIP_MSG_INPKT,
		MEMP_ARP_QUEUE,
		MEMP_IGMP_GROUP,
		MEMP_SYS_TIMEOUT,
		MEMP_SNMP_ROOTNODE,
		MEMP_SNMP_NODE,
		MEMP_SNMP_VARBIND,
		MEMP_SNMP_VALUE,
		MEMP_NETDB,
		MEMP_LOCALHOSTLIST,
		MEMP_PPPOE_IF,
		MEMP_MAX,
	}

	public enum mempb_t
	{
		MEMP_PBUF,
		MEMP_PBUF_POOL
	}

	public class memp
	{
		public const int length = 4;
		protected lwip lwip;
		internal memp_t _type;

		public memp(lwip lwip)
		{
			this.lwip = lwip;
		}

		internal static void memset(memp pcb, int c, int length)
		{
			System.Diagnostics.Debug.Assert(c == 0);
			//System.Diagnostics.Debug.Assert(pcb._type == 0);
		}
	}

	partial class lwip
	{
		internal pointer memp_malloc(mempb_t type)
		{
			switch (type) {
			case mempb_t.MEMP_PBUF:
				return new pointer(new byte[pbuf.length], 0);
			case mempb_t.MEMP_PBUF_POOL:
				return new pointer(new byte[opt.PBUF_POOL_BUFSIZE], 0);
			default:
				throw new InvalidOperationException();
			}
		}

		internal void memp_free(mempb_t type, pointer pcb)
		{

		}

		internal memp memp_malloc(memp_t type)
		{
			memp memp;

			switch (type) {
#if LWIP_RAW
				case memp_t.MEMP_RAW_PCB:
					memp = new raw_pcb(this);
					break;
#endif
#if LWIP_UDP
			case memp_t.MEMP_UDP_PCB:
				memp = new udp_pcb(this);
				break;
#endif
#if LWIP_TCP
			case memp_t.MEMP_TCP_PCB:
				memp = new tcp_pcb(this);
				break;
			case memp_t.MEMP_TCP_PCB_LISTEN:
				memp = new tcp_pcb_listen(this);
				break;
			case memp_t.MEMP_TCP_SEG:
				memp = new tcp_seg(this);
				break;
#endif
#if IP_REASSEMBLY
			case memp_t.MEMP_REASSDATA:
				memp = new ip_reassdata(this);
				break;
			case memp_t.MEMP_FRAG_PBUF:
				memp = new frag_pbuf(this);
				break;
#endif
#if LWIP_NETCONN
			case memp_t.MEMP_NETBUF:
				memp = new netbuf(this);
				break;
			case memp_t.MEMP_NETCONN:
				memp = new netconn(this);
				break;
#endif
#if false //!NO_SYS
			case memp_t.MEMP_TCPIP_MSG_API:
				memp = new tcpip_msg(this);
				break;
			case memp_t.MEMP_TCPIP_MSG_INPKT:
				memp = new tcpip_msg(this);
				break;
#endif
#if LWIP_ARP && ARP_QUEUEING
			case memp_t.MEMP_ARP_QUEUE:
				memp = new etharp_q_entry(this);
				break;
#endif
#if LWIP_IGMP
			case memp_t.MEMP_IGMP_GROUP:
				memp = new igmp_group(this);
				break;
#endif
#if false //(!NO_SYS || (NO_SYS && !NO_SYS_NO_TIMERS))
			case memp_t.MEMP_SYS_TIMEOUT:
				memp = new sys_timeo(this);
				break;
#endif
#if LWIP_SNMP
			case memp_t.MEMP_SNMP_ROOTNODE:
				memp = new mib_list_rootnode(this);
				break;
			case memp_t.MEMP_SNMP_NODE:
				memp = new mib_list_node(this);
				break;
			case memp_t.MEMP_SNMP_VARBIND:
				memp = new snmp_varbind(this);
				break;
			case memp_t.MEMP_SNMP_VALUE:
				memp = new snmp_value(this);
				break;
#endif
#if LWIP_DNS && LWIP_SOCKET
			case memp_t.MEMP_NETDB:
				memp = new netdb(this);
				break;
#endif
#if LWIP_DNS && DNS_LOCAL_HOSTLIST && DNS_LOCAL_HOSTLIST_IS_DYNAMIC
			case memp_t.MEMP_LOCALHOSTLIST:
				memp = new local_hostlist_entry(this);
				break;
#endif
#if PPP_SUPPORT && PPPOE_SUPPORT
			case memp_t.MEMP_PPPOE_IF:
				memp = new pppoe_softc(this);
				break;
#endif
			default:
				throw new InvalidOperationException();
			}

			memp._type = type;
			memp_heap.AddLast(memp);
			return memp;
		}

		internal void memp_free(memp_t type, memp pcb)
		{
			System.Diagnostics.Debug.Assert(pcb._type == type);
			memp_heap.Remove(pcb);
		}

		public static void memp_init()
		{
		}

		internal LinkedList<memp> memp_heap = new LinkedList<memp>();
	}
}
