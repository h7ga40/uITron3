/**
 * @file
 *
 * lwIP Options Configuration
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public static class opt
	{
		/*
		   -----------------------------------------------
		   ---------- Platform specific locking ----------
		   -----------------------------------------------
		*/

		/**
		 * sys.SYS_LIGHTWEIGHT_PROT==1: if you want inter-task protection for certain
		 * critical regions during buffer allocation, deallocation and memory
		 * allocation and deallocation.
		 */
#if SYS_LIGHTWEIGHT_PROT
		public const int SYS_LIGHTWEIGHT_PROT = 1;
#else
		public const int SYS_LIGHTWEIGHT_PROT = 0;
#endif

		/** 
		 * NO_SYS==1: Provides VERY minimal functionality. Otherwise,
		 * use lwIP facilities.
		 */
#if NO_SYS
		public const int NO_SYS = 1;
#else
		public const int NO_SYS = 0;
#endif

		/**
		 * NO_SYS_NO_TIMERS==1: Drop support for sys_timeout when NO_SYS==1
		 * Mainly for compatibility to old versions.
		 */
#if NO_SYS_NO_TIMERS
		public const int NO_SYS_NO_TIMERS = 1;
#else
		public const int NO_SYS_NO_TIMERS = 0;
#endif

		/**
		 * MEMCPY: override this if you have a faster implementation at hand than the
		 * one included in your C library
		 */
		public static void MEMCPY(pointer dst, pointer src, int len) { pointer.memcpy(dst, src, len); }

		/**
		 * SMEMCPY: override this with care! Some compilers (e.g. gcc) can inline a
		 * call to memcpy() if the length is known at compile time and is small.
		 */
		public static void SMEMCPY(pointer dst, pointer src, int len) { pointer.memcpy(dst, src, len); }

		/*
		   ------------------------------------
		   ---------- Memory options ----------
		   ------------------------------------
		*/
		/**
		 * MEM_LIBC_MALLOC==1: Use malloc/free/realloc provided by your C-library
		 * instead of the lwip internal allocator. Can save code size if you
		 * already use it.
		 */
#if MEM_LIBC_MALLOC
		public const int MEM_LIBC_MALLOC = 1;
#else
		public const int MEM_LIBC_MALLOC = 0;
#endif

		/**
		* MEMP_MEM_MALLOC==1: Use mem_malloc/mem_free instead of the lwip pool allocator.
		* Especially useful with MEM_LIBC_MALLOC but handle with care regarding execution
		* speed and usage from interrupts!
		*/
#if MEMP_MEM_MALLOC
		public const int MEMP_MEM_MALLOC = 1;
#else
		public const int MEMP_MEM_MALLOC = 0;
#endif

		/**
		 * MEM_ALIGNMENT: should be set to the alignment of the CPU
		 *    4 byte alignment . public const int MEM_ALIGNMENT = 4;
		 *    2 byte alignment . public const int MEM_ALIGNMENT = 2;
		 */
		public const int MEM_ALIGNMENT = 1;

		/**
		 * MEM_SIZE: the size of the heap memory. If the application will send
		 * a lot of data that needs to be copied, this should be set high.
		 */
		public const int MEM_SIZE = 1600;

		/**
		 * MEMP_SEPARATE_POOLS: if defined to 1, each pool is placed in its own array.
		 * This can be used to individually change the location of each pool.
		 * Default is one big array for all pools
		 */
#if MEMP_SEPARATE_POOLS
		public const int MEMP_SEPARATE_POOLS = 1;
#else
		public const int MEMP_SEPARATE_POOLS = 0;
#endif

		/**
		 * MEMP_OVERFLOW_CHECK: memp overflow protection reserves a configurable
		 * amount of bytes before and after each memp element in every pool and fills
		 * it with a prominent default value.
		 *    MEMP_OVERFLOW_CHECK == 0 no checking
		 *    MEMP_OVERFLOW_CHECK == 1 checks each element when it is freed
		 *    MEMP_OVERFLOW_CHECK >= 2 checks each element in every pool every time
		 *      lwip.memp_malloc() or lwip.memp_free() is called (useful but slow!)
		 */
#if MEMP_OVERFLOW_CHECK
		public const int MEMP_OVERFLOW_CHECK = 1;
#else
		public const int MEMP_OVERFLOW_CHECK = 0;
#endif

		/**
		 * MEMP_SANITY_CHECK==1: run a sanity check after each lwip.memp_free() to make
		 * sure that there are no cycles in the linked lists.
		 */
#if MEMP_SANITY_CHECK
		public const int MEMP_SANITY_CHECK = 1;
#else
		public const int MEMP_SANITY_CHECK = 0;
#endif

		/**
		 * MEM_USE_POOLS==1: Use an alternative to malloc() by allocating from a set
		 * of memory pools of various sizes. When mem_malloc is called, an element of
		 * the smallest pool that can provide the length needed is returned.
		 * To use this, MEMP_USE_CUSTOM_POOLS also has to be enabled.
		 */
#if MEM_USE_POOLS
		public const int MEM_USE_POOLS = 1;
#else
		public const int MEM_USE_POOLS = 0;
#endif

		/**
		 * MEM_USE_POOLS_TRY_BIGGER_POOL==1: if one malloc-pool is empty, try the next
		 * bigger pool - WARNING: THIS MIGHT WASTE MEMORY but it can make a system more
		 * reliable. */
#if MEM_USE_POOLS_TRY_BIGGER_POOL
		public const int MEM_USE_POOLS_TRY_BIGGER_POOL = 1;
#else
		public const int MEM_USE_POOLS_TRY_BIGGER_POOL = 0;
#endif

		/**
		 * MEMP_USE_CUSTOM_POOLS==1: whether to include a user file lwippools.h
		 * that defines additional pools beyond the "standard" ones required
		 * by lwIP. If you set this to 1, you must have lwippools.h in your 
		 * inlude path somewhere. 
		 */
#if MEMP_USE_CUSTOM_POOLS
		public const int MEMP_USE_CUSTOM_POOLS = 1;
#else
		public const int MEMP_USE_CUSTOM_POOLS = 0;
#endif

		/**
		 * Set this to 1 if you want to free pbuf_type.PBUF_RAM pbufs (or call mem_free()) from
		 * interrupt context (or another context that doesn't allow waiting for a
		 * semaphore).
		 * If set to 1, mem_malloc will be protected by a semaphore and sys.SYS_ARCH_PROTECT,
		 * while mem_free will only use sys.SYS_ARCH_PROTECT. mem_malloc sys.SYS_ARCH_UNPROTECTs
		 * with each loop so that mem_free can run.
		 *
		 * ATTENTION: As you can see from the above description, this leads to dis-/
		 * enabling interrupts often, which can be slow! Also, on low memory, mem_malloc
		 * can need longer.
		 *
		 * If you don't want that, at least for NO_SYS=0, you can still use the following
		 * functions to enqueue a deallocation call which then runs in the tcpip_thread
		 * context:
		 * - pbuf_free_callback(p);
		 * - mem_free_callback(m);
		 */
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
		public const int LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT = 1;
#else
		public const int LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT = 0;
#endif

		/*
		   ------------------------------------------------
		   ---------- Internal Memory Pool Sizes ----------
		   ------------------------------------------------
		*/
		/**
		 * MEMP_NUM_PBUF: the number of memp pbufs (used for pbuf_type.PBUF_ROM and pbuf_type.PBUF_REF).
		 * If the application sends a lot of data out of ROM (or other static memory),
		 * this should be set high.
		 */
		public const int MEMP_NUM_PBUF = 16;

		/**
		 * MEMP_NUM_RAW_PCB: Number of raw connection PCBs
		 * (requires the LWIP_RAW option)
		 */
		public const int MEMP_NUM_RAW_PCB = 4;

		/**
		 * MEMP_NUM_UDP_PCB: the number of UDP protocol control blocks. One
		 * per active UDP "connection".
		 * (requires the LWIP_UDP option)
		 */
		public const int MEMP_NUM_UDP_PCB = 4;

		/**
		 * MEMP_NUM_TCP_PCB: the number of simulatenously active TCP connections.
		 * (requires the LWIP_TCP option)
		 */
		public const int MEMP_NUM_TCP_PCB = 5;

		/**
		 * MEMP_NUM_TCP_PCB_LISTEN: the number of listening TCP connections.
		 * (requires the LWIP_TCP option)
		 */
		public const int MEMP_NUM_TCP_PCB_LISTEN = 8;

		/**
		 * MEMP_NUM_TCP_SEG: the number of simultaneously queued TCP segments.
		 * (requires the LWIP_TCP option)
		 */
		public const int MEMP_NUM_TCP_SEG = 16;

		/**
		 * MEMP_NUM_REASSDATA: the number of IP packets simultaneously queued for
		 * reassembly (whole packets, not fragments!)
		 */
		public const int MEMP_NUM_REASSDATA = 5;

		/**
		 * MEMP_NUM_FRAG_PBUF: the number of IP fragments simultaneously sent
		 * (fragments, not whole packets!).
		 * This is only used with IP_FRAG_USES_STATIC_BUF==0 and
		 * LWIP_NETIF_TX_SINGLE_PBUF==0 and only has to be > 1 with DMA-enabled MACs
		 * where the packet is not yet sent when netif.output returns.
		 */
		public const int MEMP_NUM_FRAG_PBUF = 15;

		/**
		 * MEMP_NUM_ARP_QUEUE: the number of simulateously queued outgoing
		 * packets (pbufs) that are waiting for an ARP request (to resolve
		 * their destination address) to finish.
		 * (requires the ARP_QUEUEING option)
		 */
		public const int MEMP_NUM_ARP_QUEUE = 30;

		/**
		 * MEMP_NUM_IGMP_GROUP: The number of multicast groups whose network interfaces
		 * can be members et the same time (one per netif - allsystems group -, plus one
		 * per netif membership).
		 * (requires the LWIP_IGMP option)
		 */
		public const int MEMP_NUM_IGMP_GROUP = 8;

		/**
		 * MEMP_NUM_SYS_TIMEOUT: the number of simulateously active timeouts.
		 * (requires NO_SYS==0)
		 * The default number of timeouts is calculated here for all enabled modules.
		 * The formula expects settings to be either '0' or '1'.
		 */
		public const int MEMP_NUM_SYS_TIMEOUT = (LWIP_TCP + IP_REASSEMBLY + LWIP_ARP + (2 * LWIP_DHCP) + LWIP_AUTOIP + LWIP_IGMP + LWIP_DNS + PPP_SUPPORT);

		/**
		 * MEMP_NUM_NETBUF: the number of netbufs.
		 * (only needed if you use the sequential API, like api_lib.c)
		 */
		public const int MEMP_NUM_NETBUF = 2;

		/**
		 * MEMP_NUM_NETCONN: the number of netconns.
		 * (only needed if you use the sequential API, like api_lib.c)
		 */
		public const int MEMP_NUM_NETCONN = 4;

		/**
		 * MEMP_NUM_TCPIP_MSG_API: the number of tcpip_msg, which are used
		 * for callback/timeout API communication. 
		 * (only needed if you use tcpip.c)
		 */
		public const int MEMP_NUM_TCPIP_MSG_API = 8;

		/**
		 * MEMP_NUM_TCPIP_MSG_INPKT: the number of tcpip_msg, which are used
		 * for incoming packets. 
		 * (only needed if you use tcpip.c)
		 */
		public const int MEMP_NUM_TCPIP_MSG_INPKT = 8;

		/**
		 * MEMP_NUM_SNMP_NODE: the number of leafs in the SNMP tree.
		 */
		public const int MEMP_NUM_SNMP_NODE = 50;

		/**
		 * MEMP_NUM_SNMP_ROOTNODE: the number of branches in the SNMP tree.
		 * Every branch has one leaf (MEMP_NUM_SNMP_NODE) at least!
		 */
		public const int MEMP_NUM_SNMP_ROOTNODE = 30;

		/**
		 * MEMP_NUM_SNMP_VARBIND: the number of concurrent requests (does not have to
		 * be changed normally) - 2 of these are used per request (1 for input,
		 * 1 for output)
		 */
		public const int MEMP_NUM_SNMP_VARBIND = 2;

		/**
		 * MEMP_NUM_SNMP_VALUE: the number of OID or values concurrently used
		 * (does not have to be changed normally) - 3 of these are used per request
		 * (1 for the value read and 2 for OIDs - input and output)
		 */
		public const int MEMP_NUM_SNMP_VALUE = 3;

		/**
		 * MEMP_NUM_NETDB: the number of concurrently running lwip_addrinfo() calls
		 * (before freeing the corresponding memory using lwip_freeaddrinfo()).
		 */
		public const int MEMP_NUM_NETDB = 1;

		/**
		 * MEMP_NUM_LOCALHOSTLIST: the number of host entries in the local host list
		 * if DNS_LOCAL_HOSTLIST_IS_DYNAMIC==1.
		 */
		public const int MEMP_NUM_LOCALHOSTLIST = 1;

		/**
		 * MEMP_NUM_PPPOE_INTERFACES: the number of concurrently active PPPoE
		 * interfaces (only used with PPPOE_SUPPORT==1)
		 */
		public const int MEMP_NUM_PPPOE_INTERFACES = 1;

		/**
		 * PBUF_POOL_SIZE: the number of buffers in the pbuf pool. 
		 */
		public const int PBUF_POOL_SIZE = 16;

		/*
		   ---------------------------------
		   ---------- ARP options ----------
		   ---------------------------------
		*/
		/**
		 * LWIP_ARP==1: Enable ARP functionality.
		 */
#if LWIP_ARP
		public const int LWIP_ARP = 1;
#else
		public const int LWIP_ARP = 0;
#endif

		/**
		 * ARP_TABLE_SIZE: Number of active MAC-IP address pairs cached.
		 */
		public const int ARP_TABLE_SIZE = 10;

		/**
		 * ARP_QUEUEING==1: Multiple outgoing packets are queued during hardware address
		 * resolution. By default, only the most recent packet is queued per IP address.
		 * This is sufficient for most protocols and mainly reduces TCP connection
		 * startup time. Set this to 1 if you know your application sends more than one
		 * packet in a row to an IP address that is not in the ARP cache.
		 */
#if ARP_QUEUEING
		public const int ARP_QUEUEING = 1;
#else
		public const int ARP_QUEUEING = 0;
#endif

		/**
		 * ETHARP_TRUST_IP_MAC==1: Incoming IP packets cause the ARP table to be
		 * updated with the source MAC and IP addresses supplied in the packet.
		 * You may want to disable this if you do not trust LAN peers to have the
		 * correct addresses, or as a limited approach to attempt to handle
		 * spoofing. If disabled, lwIP will need to make a new ARP request if
		 * the peer is not already in the ARP table, adding a little latency.
		 * The peer is in the ARP table if it requested our address before.
		 * Also notice that this slows down input processing of every IP packet!
		 */
#if ETHARP_TRUST_IP_MAC
		public const int ETHARP_TRUST_IP_MAC = 1;
#else
		public const int ETHARP_TRUST_IP_MAC = 0;
#endif

		/**
		 * ETHARP_SUPPORT_VLAN==1: support receiving ethernet packets with VLAN header.
		 * Additionally, you can define ETHARP_VLAN_CHECK to an ushort VLAN ID to check.
		 * If ETHARP_VLAN_CHECK is defined, only VLAN-traffic for this VLAN is accepted.
		 * If ETHARP_VLAN_CHECK is not defined, all traffic is accepted.
		 * Alternatively, define a function/define ETHARP_VLAN_CHECK_FN(eth_hdr, vlan)
		 * that returns 1 to accept a packet or 0 to drop a packet.
		 */
#if ETHARP_SUPPORT_VLAN
		public const int ETHARP_SUPPORT_VLAN = 1;
#else
		public const int ETHARP_SUPPORT_VLAN = 0;
#endif

		/** LWIP_ETHERNET==1: enable ethernet support for PPPoE even though ARP
		 * might be disabled
		 */
		public const bool LWIP_ETHERNET = (LWIP_ARP != 0) || (PPPOE_SUPPORT != 0);

		/** ETH_PAD_SIZE: number of bytes added before the ethernet header to ensure
		 * alignment of payload after that header. Since the header is 14 bytes long,
		 * without this padding e.g. addresses in the IP header will not be aligned
		 * on a 32-bit boundary, so setting this to 2 can speed up 32-bit-platforms.
		 */
		public const int ETH_PAD_SIZE = 0;

		/** ETHARP_SUPPORT_STATIC_ENTRIES==1: enable code to support static ARP table
		 * entries (using etharp_add_static_entry/etharp_remove_static_entry).
		 */
#if ETHARP_SUPPORT_STATIC_ENTRIES
		public const int ETHARP_SUPPORT_STATIC_ENTRIES = 1;
#else
		public const int ETHARP_SUPPORT_STATIC_ENTRIES = 0;
#endif


		/*
		   --------------------------------
		   ---------- IP options ----------
		   --------------------------------
		*/
		/**
		 * IP_FORWARD==1: Enables the ability to forward IP packets across network
		 * interfaces. If you are going to run lwIP on a device with only one network
		 * interface, define this to 0.
		 */
#if IP_FORWARD
		public const int IP_FORWARD = 1;
#else
		public const int IP_FORWARD = 0;
#endif

		/**
		 * IP_OPTIONS_ALLOWED: Defines the behavior for IP options.
		 *      IP_OPTIONS_ALLOWED==0: All packets with IP options are dropped.
		 *      IP_OPTIONS_ALLOWED==1: IP options are allowed (but not parsed).
		 */
#if IP_OPTIONS_ALLOWED
		public const int IP_OPTIONS_ALLOWED = 1;
#else
		public const int IP_OPTIONS_ALLOWED = 0;
#endif

		/**
		 * IP_REASSEMBLY==1: Reassemble incoming fragmented IP packets. Note that
		 * this option does not affect outgoing packet sizes, which can be controlled
		 * via IP_FRAG.
		 */
#if IP_REASSEMBLY
		public const int IP_REASSEMBLY = 1;
#else
		public const int IP_REASSEMBLY = 0;
#endif

		/**
		 * IP_FRAG==1: Fragment outgoing IP packets if their size exceeds MTU. Note
		 * that this option does not affect incoming packet sizes, which can be
		 * controlled via IP_REASSEMBLY.
		 */
#if IP_FRAG
		public const int IP_FRAG = 1;
#else
		public const int IP_FRAG = 0;
#endif

		/**
		 * IP_REASS_MAXAGE: Maximum time (in multiples of IP_TMR_INTERVAL - so seconds, normally)
		 * a fragmented IP packet waits for all fragments to arrive. If not all fragments arrived
		 * in this time, the whole packet is discarded.
		 */
		public const int IP_REASS_MAXAGE = 3;

		/**
		 * IP_REASS_MAX_PBUFS: Total maximum amount of pbufs waiting to be reassembled.
		 * Since the received pbufs are enqueued, be sure to configure
		 * PBUF_POOL_SIZE > IP_REASS_MAX_PBUFS so that the stack is still able to receive
		 * packets even if the maximum amount of fragments is enqueued for reassembly!
		 */
		public const int IP_REASS_MAX_PBUFS = 10;

		/**
		 * IP_FRAG_USES_STATIC_BUF==1: Use a static MTU-sized buffer for IP
		 * fragmentation. Otherwise pbufs are allocated and reference the original
		 * packet data to be fragmented (or with LWIP_NETIF_TX_SINGLE_PBUF==1,
		 * new pbuf_type.PBUF_RAM pbufs are used for fragments).
		 * ATTENTION: IP_FRAG_USES_STATIC_BUF==1 may not be used for DMA-enabled MACs!
		 */
#if IP_FRAG_USES_STATIC_BUF
		public const int IP_FRAG_USES_STATIC_BUF = 0;
#else
		public const int IP_FRAG_USES_STATIC_BUF = 1;
#endif

		/**
		 * IP_FRAG_MAX_MTU: Assumed max MTU on any interface for IP frag buffer
		 * (requires IP_FRAG_USES_STATIC_BUF==1)
		 */
		public const int IP_FRAG_MAX_MTU = 1500;

		/**
		 * IP_DEFAULT_TTL: Default value for Time-To-Live used by transport layers.
		 */
		public const int IP_DEFAULT_TTL = 255;

		/**
		 * IP_SOF_BROADCAST=1: Use the sof.SOF_BROADCAST field to enable broadcast
		 * filter per pcb on udp and raw send operations. To enable broadcast filter
		 * on recv operations, you also have to set IP_SOF_BROADCAST_RECV=1.
		 */
#if IP_SOF_BROADCAST
		public const int IP_SOF_BROADCAST = 1;
#else
		public const int IP_SOF_BROADCAST = 0;
#endif

		/**
		 * IP_SOF_BROADCAST_RECV (requires IP_SOF_BROADCAST=1) enable the broadcast
		 * filter on recv operations.
		 */
#if IP_SOF_BROADCAST_RECV
		public const int IP_SOF_BROADCAST_RECV = 1;
#else
		public const int IP_SOF_BROADCAST_RECV = 0;
#endif

		/**
		 * IP_FORWARD_ALLOW_TX_ON_RX_NETIF==1: allow ip_forward() to send packets back
		 * out on the netif where it was received. This should only be used for
		 * wireless networks.
		 * ATTENTION: When this is 1, make sure your netif driver correctly marks incoming
		 * link-layer-broadcast/multicast packets as such using the corresponding pbuf flags!
		 */
#if IP_FORWARD_ALLOW_TX_ON_RX_NETIF
		public const int IP_FORWARD_ALLOW_TX_ON_RX_NETIF = 1;
#else
		public const int IP_FORWARD_ALLOW_TX_ON_RX_NETIF = 0;
#endif

		/**
		 * LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS==1: randomize the local port for the first
		 * local TCP/UDP pcb (default==0). This can prevent creating predictable port
		 * numbers after booting a device.
		 */
#if LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS
		public const int LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS = 1;
#else
		public const int LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS = 0;
#endif

		/*
		   ----------------------------------
		   ---------- ICMP options ----------
		   ----------------------------------
		*/
		/**
		 * LWIP_ICMP==1: Enable ICMP module inside the IP stack.
		 * Be careful, disable that make your product non-compliant to RFC1122
		 */
#if LWIP_ICMP
		public const int LWIP_ICMP = 1;
#else
		public const int LWIP_ICMP = 0;
#endif

		/**
		 * ICMP_TTL: Default value for Time-To-Live used by ICMP packets.
		 */
		public const int ICMP_TTL = (IP_DEFAULT_TTL);

		/**
		 * LWIP_BROADCAST_PING==1: respond to broadcast pings (default is unicast only)
		 */
#if LWIP_BROADCAST_PING
		public const int LWIP_BROADCAST_PING = 1;
#else
		public const int LWIP_BROADCAST_PING = 0;
#endif

		/**
		 * LWIP_MULTICAST_PING==1: respond to multicast pings (default is unicast only)
		 */
#if LWIP_MULTICAST_PING
		public const int LWIP_MULTICAST_PING = 1;
#else
		public const int LWIP_MULTICAST_PING = 0;
#endif

		/*
		   ---------------------------------
		   ---------- RAW options ----------
		   ---------------------------------
		*/
		/**
		 * LWIP_RAW==1: Enable application layer to hook into the IP layer itself.
		 */
#if LWIP_RAW
		public const int LWIP_RAW = 1;
#else
		public const int LWIP_RAW = 0;
#endif

		/**
		 * LWIP_RAW==1: Enable application layer to hook into the IP layer itself.
		 */
		public const int RAW_TTL = (IP_DEFAULT_TTL);

		/*
		   ----------------------------------
		   ---------- DHCP options ----------
		   ----------------------------------
		*/
		/**
		 * LWIP_DHCP==1: Enable DHCP module.
		 */
#if LWIP_DHCP
		public const int LWIP_DHCP = 1;
#else
		public const int LWIP_DHCP = 0;
#endif

		/**
		 * DHCP_DOES_ARP_CHECK==1: Do an ARP check on the offered address.
		 */
		public const int DHCP_DOES_ARP_CHECK = ((LWIP_DHCP) & (LWIP_ARP));

		/*
		   ------------------------------------
		   ---------- AUTOIP options ----------
		   ------------------------------------
		*/
		/**
		 * LWIP_AUTOIP==1: Enable AUTOIP module.
		 */
#if LWIP_AUTOIP
		public const int LWIP_AUTOIP = 1;
#else
		public const int LWIP_AUTOIP = 0;
#endif

		/**
		 * LWIP_DHCP_AUTOIP_COOP==1: Allow DHCP and AUTOIP to be both enabled on
		 * the same interface at the same time.
		 */
#if LWIP_DHCP_AUTOIP_COOP
		public const int LWIP_DHCP_AUTOIP_COOP = 1;
#else
		public const int LWIP_DHCP_AUTOIP_COOP = 0;
#endif

		/**
		 * LWIP_DHCP_AUTOIP_COOP_TRIES: Set to the number of DHCP DISCOVER probes
		 * that should be sent before falling back on AUTOIP. This can be set
		 * as low as 1 to get an AutoIP address very quickly, but you should
		 * be prepared to handle a changing IP address when DHCP overrides
		 * AutoIP.
		 */
		public const int LWIP_DHCP_AUTOIP_COOP_TRIES = 9;

		/*
		   ----------------------------------
		   ---------- SNMP options ----------
		   ----------------------------------
		*/
		/**
		 * LWIP_SNMP==1: Turn on SNMP module. UDP must be available for SNMP
		 * transport.
		 */
#if LWIP_SNMP
		public const int LWIP_SNMP = 1;
#else
		public const int LWIP_SNMP = 0;
#endif

		/**
		 * SNMP_CONCURRENT_REQUESTS: Number of concurrent requests the module will
		 * allow. At least one request buffer is required.
		 * Does not have to be changed unless external MIBs answer request asynchronously
		 */
#if SNMP_CONCURRENT_REQUESTS
		public const int SNMP_CONCURRENT_REQUESTS = 1;
#else
		public const int SNMP_CONCURRENT_REQUESTS = 0;
#endif

		/**
		 * SNMP_TRAP_DESTINATIONS: Number of trap destinations. At least one trap
		 * destination is required
		 */
#if SNMP_TRAP_DESTINATIONS
		public const int SNMP_TRAP_DESTINATIONS = 1;
#else
		public const int SNMP_TRAP_DESTINATIONS = 0;
#endif

		/**
		 * SNMP_PRIVATE_MIB: 
		 * When using a private MIB, you have to create a file 'private_mib.h' that contains
		 * a 'mib_array_node mib_private' which contains your MIB.
		 */
#if SNMP_PRIVATE_MIB
		public const int SNMP_PRIVATE_MIB = 1;
#else
		public const int SNMP_PRIVATE_MIB = 0;
#endif

		/**
		 * Only allow SNMP write actions that are 'safe' (e.g. disabeling netifs is not
		 * a safe action and disabled when SNMP_SAFE_REQUESTS = 1).
		 * Unsafe requests are disabled by default!
		 */
#if SNMP_SAFE_REQUESTS
		public const int SNMP_SAFE_REQUESTS = 1;
#else
		public const int SNMP_SAFE_REQUESTS = 0;
#endif

		/**
		 * The maximum length of strings used. This affects the size of
		 * MEMP_SNMP_VALUE elements.
		 */
		public const int SNMP_MAX_OCTET_STRING_LEN = 127;

		/**
		 * The maximum depth of the SNMP tree.
		 * With private MIBs enabled, this depends on your MIB!
		 * This affects the size of MEMP_SNMP_VALUE elements.
		 */
		public const int SNMP_MAX_TREE_DEPTH = 15;

		/**
		 * The size of the MEMP_SNMP_VALUE elements, normally calculated from
		 * SNMP_MAX_OCTET_STRING_LEN and SNMP_MAX_TREE_DEPTH.
		 */
		public static readonly int SNMP_MAX_VALUE_SIZE = Math.Max((SNMP_MAX_OCTET_STRING_LEN) + 1, sizeof(int) * (SNMP_MAX_TREE_DEPTH));

		/*
		   ----------------------------------
		   ---------- IGMP options ----------
		   ----------------------------------
		*/
		/**
		 * LWIP_IGMP==1: Turn on IGMP module. 
		 */
#if LWIP_IGMP
		public const int LWIP_IGMP = 1;
#else
		public const int LWIP_IGMP = 0;
#endif

		/*
		   ----------------------------------
		   ---------- DNS options -----------
		   ----------------------------------
		*/
		/**
		 * LWIP_DNS==1: Turn on DNS module. UDP must be available for DNS
		 * transport.
		 */
#if LWIP_DNS
		public const int LWIP_DNS = 1;
#else
		public const int LWIP_DNS = 0;
#endif

		/** DNS maximum number of entries to maintain locally. */
		public const int DNS_TABLE_SIZE = 4;

		/** DNS maximum host name length supported in the name table. */
		public const int DNS_MAX_NAME_LENGTH = 256;

		/** The maximum of DNS servers */
		public const int DNS_MAX_SERVERS = 2;

		/** DNS do a name checking between the query and the response. */
#if DNS_DOES_NAME_CHECK
		public const int DNS_DOES_NAME_CHECK = 1;
#else
		public const int DNS_DOES_NAME_CHECK = 0;
#endif

		/** DNS message max. size. Default value is RFC compliant. */
		public const int DNS_MSG_SIZE = 512;

		/** DNS_LOCAL_HOSTLIST: Implements a local host-to-address list. If enabled,
		 *  you have to define
		 *    public const int DNS_LOCAL_HOSTLIST_INIT = {{"host1", 0x123}, {"host2", 0x234}};
		 *  (an array of structs name/address, where address is an uint in network
		 *  byte order).
		 *
		 *  Instead, you can also use an external function:
		 *  public const int DNS_LOOKUP_LOCAL_EXTERN(x) = extern uint my_lookup_function(pointer name);
		 *  that returns the IP address or INADDR_NONE if not found.
		 */
#if DNS_LOCAL_HOSTLIST
		public const int DNS_LOCAL_HOSTLIST = 1;
#else
		public const int DNS_LOCAL_HOSTLIST = 0;
#endif

		/** If this is turned on, the local host-list can be dynamically changed
		 *  at runtime. */
#if DNS_LOCAL_HOSTLIST_IS_DYNAMIC
		public const int DNS_LOCAL_HOSTLIST_IS_DYNAMIC = 1;
#else
		public const int DNS_LOCAL_HOSTLIST_IS_DYNAMIC = 0;
#endif

		/*
		   ---------------------------------
		   ---------- UDP options ----------
		   ---------------------------------
		*/
		/**
		 * LWIP_UDP==1: Turn on UDP.
		 */
#if LWIP_UDP
		public const int LWIP_UDP = 1;
#else
		public const int LWIP_UDP = 0;
#endif

		/**
		 * LWIP_UDPLITE==1: Turn on UDP-Lite. (Requires LWIP_UDP)
		 */
#if LWIP_UDPLITE
		public const int LWIP_UDPLITE = 1;
#else
		public const int LWIP_UDPLITE = 0;
#endif

		/**
		 * UDP_TTL: Default Time-To-Live value.
		 */
		public const int UDP_TTL = (IP_DEFAULT_TTL);

		/**
		 * LWIP_NETBUF_RECVINFO==1: append destination addr and port to every netbuf.
		 */
#if LWIP_NETBUF_RECVINFO
		public const int LWIP_NETBUF_RECVINFO = 1;
#else
		public const int LWIP_NETBUF_RECVINFO = 0;
#endif

		/*
		   ---------------------------------
		   ---------- TCP options ----------
		   ---------------------------------
		*/
		/**
		 * LWIP_TCP==1: Turn on TCP.
		 */
#if LWIP_TCP
		public const int LWIP_TCP = 1;
#else
		public const int LWIP_TCP = 0;
#endif

		/**
		 * TCP_TTL: Default Time-To-Live value.
		 */
		public const int TCP_TTL = (IP_DEFAULT_TTL);

		/**
		 * TCP_WND: The size of a TCP window.  This must be at least 
		 * (2 * TCP_MSS) for things to work well
		 */
		public const int TCP_WND = (4 * TCP_MSS);

		/**
		 * TCP_MAXRTX: Maximum number of retransmissions of data segments.
		 */
		public const int TCP_MAXRTX = 12;

		/**
		 * TCP_SYNMAXRTX: Maximum number of retransmissions of SYN segments.
		 */
		public const int TCP_SYNMAXRTX = 6;

		/**
		 * TCP_QUEUE_OOSEQ==1: TCP will queue segments that arrive out of order.
		 * Define to 0 if your device is low on memory.
		 */
		public const int TCP_QUEUE_OOSEQ = (LWIP_TCP);

		/**
		 * TCP_MSS: TCP Maximum segment size. (default is 536, a conservative default,
		 * you might want to increase this.)
		 * For the receive side, this MSS is advertised to the remote side
		 * when opening a connection. For the transmit size, this MSS sets
		 * an upper limit on the MSS advertised by the remote host.
		 */
		public const int TCP_MSS = 536;

		/**
		 * TCP_CALCULATE_EFF_SEND_MSS: "The maximum size of a segment that TCP really
		 * sends, the 'effective send MSS,' MUST be the smaller of the send MSS (which
		 * reflects the available reassembly buffer size at the remote host) and the
		 * largest size permitted by the IP layer" (RFC 1122)
		 * Setting this to 1 enables code that checks TCP_MSS against the MTU of the
		 * netif used for a connection and limits the MSS if it would be too big otherwise.
		 */
#if TCP_CALCULATE_EFF_SEND_MSS
		public const int TCP_CALCULATE_EFF_SEND_MSS = 1;
#else
		public const int TCP_CALCULATE_EFF_SEND_MSS = 0;
#endif


		/**
		 * TCP_SND_BUF: TCP sender buffer space (bytes).
		 * To achieve good performance, this should be at least 2 * TCP_MSS.
		 */
		public const int TCP_SND_BUF = (2 * TCP_MSS);

		/**
		 * TCP_SND_QUEUELEN: TCP sender buffer space (pbufs). This must be at least
		 * as much as (2 * TCP_SND_BUF/TCP_MSS) for things to work.
		 */
		public const int TCP_SND_QUEUELEN = ((4 * (TCP_SND_BUF) + (TCP_MSS - 1)) / (TCP_MSS));

		/**
		 * TCP_SNDLOWAT: TCP writable space (bytes). This must be less than
		 * TCP_SND_BUF. It is the amount of space which must be available in the
		 * TCP snd_buf for select to return writable (combined with TCP_SNDQUEUELOWAT).
		 */
		public static readonly int TCP_SNDLOWAT = Math.Min(Math.Max(((TCP_SND_BUF) / 2), (2 * TCP_MSS) + 1), (TCP_SND_BUF) - 1);

		/**
		 * TCP_SNDQUEUELOWAT: TCP writable bufs (pbuf count). This must be less
		 * than TCP_SND_QUEUELEN. If the number of pbufs queued on a pcb drops below
		 * this number, select returns writable (combined with TCP_SNDLOWAT).
		 */
		public static readonly int TCP_SNDQUEUELOWAT = Math.Max(((TCP_SND_QUEUELEN) / 2), 5);

		/**
		 * TCP_OOSEQ_MAX_BYTES: The maximum number of bytes queued on ooseq per pcb.
		 * Default is 0 (no limit). Only valid for TCP_QUEUE_OOSEQ==0.
		 */
#if TCP_OOSEQ_MAX_BYTES
		public const int TCP_OOSEQ_MAX_BYTES = 1;
#else
		public const int TCP_OOSEQ_MAX_BYTES = 0;
#endif

		/**
		 * TCP_OOSEQ_MAX_PBUFS: The maximum number of pbufs queued on ooseq per pcb.
		 * Default is 0 (no limit). Only valid for TCP_QUEUE_OOSEQ==0.
		 */
#if TCP_OOSEQ_MAX_PBUFS
		public const int TCP_OOSEQ_MAX_PBUFS = 1;
#else
		public const int TCP_OOSEQ_MAX_PBUFS = 0;
#endif

		/**
		 * TCP_LISTEN_BACKLOG: Enable the backlog option for tcp listen pcb.
		 */
#if TCP_LISTEN_BACKLOG
		public const int TCP_LISTEN_BACKLOG = 1;
#else
		public const int TCP_LISTEN_BACKLOG = 0;
#endif

		/**
		 * The maximum allowed backlog for TCP listen netconns.
		 * This backlog is used unless another is explicitly specified.
		 * 0xff is the maximum (byte).
		 */
		public const int TCP_DEFAULT_LISTEN_BACKLOG = 0xff;

		/**
		 * TCP_OVERSIZE: The maximum number of bytes that tcp_write may
		 * allocate ahead of time in an attempt to create shorter pbuf chains
		 * for transmission. The meaningful range is 0 to TCP_MSS. Some
		 * suggested values are:
		 *
		 * 0:         Disable oversized allocation. Each tcp_write() allocates a new
					  pbuf (old behaviour).
		 * 1:         Allocate size-aligned pbufs with minimal excess. Use this if your
		 *            scatter-gather DMA requires aligned fragments.
		 * 128:       Limit the pbuf/memory overhead to 20%.
		 * TCP_MSS:   Try to create unfragmented TCP packets.
		 * TCP_MSS/4: Try to create 4 fragments or less per TCP packet.
		 */
		public const int TCP_OVERSIZE = TCP_MSS;

		/**
		 * LWIP_TCP_TIMESTAMPS==1: support the TCP timestamp option.
		 */
#if LWIP_TCP_TIMESTAMPS
		public const int LWIP_TCP_TIMESTAMPS = 1;
#else
		public const int LWIP_TCP_TIMESTAMPS = 0;
#endif

		/**
		 * TCP_WND_UPDATE_THRESHOLD: difference in window to trigger an
		 * explicit window update
		 */
		public const int TCP_WND_UPDATE_THRESHOLD = (opt.TCP_WND / 4);

		/**
		 * lwip_event.LWIP_EVENT_API and LWIP_CALLBACK_API: Only one of these should be set to 1.
		 *     lwip_event.LWIP_EVENT_API==1: The user defines lwip_tcp_event() to receive all
		 *         events (accept, sent, etc) that happen in the system.
		 *     LWIP_CALLBACK_API==1: The PCB callback function is called directly
		 *         for the event. This is the default.
		 */
#if LWIP_EVENT_API
		public const int LWIP_EVENT_API = 1;
#else
		public const int LWIP_EVENT_API = 0;
#endif
#if LWIP_CALLBACK_API
		public const int LWIP_CALLBACK_API = 1;
#else
		public const int LWIP_CALLBACK_API = 0;
#endif

		/*
		   ----------------------------------
		   ---------- Pbuf options ----------
		   ----------------------------------
		*/
		/**
		 * PBUF_LINK_HLEN: the number of bytes that should be allocated for a
		 * link level header. The default is 14, the standard value for
		 * Ethernet.
		 */
		public const int PBUF_LINK_HLEN = (14 + ETH_PAD_SIZE);

		/**
		 * PBUF_POOL_BUFSIZE: the size of each pbuf in the pbuf pool. The default is
		 * designed to accomodate single full size TCP frame in one pbuf, including
		 * TCP_MSS, IP header, and link header.
		 */
		public static readonly int PBUF_POOL_BUFSIZE = lwip.LWIP_MEM_ALIGN_SIZE(TCP_MSS + 40 + PBUF_LINK_HLEN);

		/*
		   ------------------------------------------------
		   ---------- Network Interfaces options ----------
		   ------------------------------------------------
		*/
		/**
		 * LWIP_NETIF_HOSTNAME==1: use DHCP_OPTION_HOSTNAME with netif's hostname
		 * field.
		 */
#if LWIP_NETIF_HOSTNAME
		public const int LWIP_NETIF_HOSTNAME = 1;
#else
		public const int LWIP_NETIF_HOSTNAME = 0;
#endif

		/**
		 * LWIP_NETIF_API==1: Support netif api (in netifapi.c)
		 */
#if LWIP_NETIF_API
		public const int LWIP_NETIF_API = 1;
#else
		public const int LWIP_NETIF_API = 0;
#endif

		/**
		 * LWIP_NETIF_STATUS_CALLBACK==1: Support a callback function whenever an interface
		 * changes its up/down status (i.e., due to DHCP IP acquistion)
		 */
#if LWIP_NETIF_STATUS_CALLBACK
		public const int LWIP_NETIF_STATUS_CALLBACK = 1;
#else
		public const int LWIP_NETIF_STATUS_CALLBACK = 0;
#endif

		/**
		 * LWIP_NETIF_LINK_CALLBACK==1: Support a callback function from an interface
		 * whenever the link changes (i.e., link down)
		 */
#if LWIP_NETIF_LINK_CALLBACK
		public const int LWIP_NETIF_LINK_CALLBACK = 1;
#else
		public const int LWIP_NETIF_LINK_CALLBACK = 0;
#endif

		/**
		 * LWIP_NETIF_REMOVE_CALLBACK==1: Support a callback function that is called
		 * when a netif has been removed
		 */
#if LWIP_NETIF_REMOVE_CALLBACK
		public const int LWIP_NETIF_REMOVE_CALLBACK = 1;
#else
		public const int LWIP_NETIF_REMOVE_CALLBACK = 0;
#endif

		/**
		 * LWIP_NETIF_HWADDRHINT==1: Cache link-layer-address hints (e.g. table
		 * indices) in netif. TCP and UDP can make use of this to prevent
		 * scanning the ARP table for every sent packet. While this is faster for big
		 * ARP tables or many concurrent connections, it might be counterproductive
		 * if you have a tiny ARP table or if there never are concurrent connections.
		 */
#if LWIP_NETIF_HWADDRHINT
		public const int LWIP_NETIF_HWADDRHINT = 1;
#else
		public const int LWIP_NETIF_HWADDRHINT = 0;
#endif

		/**
		 * LWIP_NETIF_LOOPBACK==1: Support sending packets with a destination IP
		 * address equal to the netif IP address, looping them back up the stack.
		 */
#if LWIP_NETIF_LOOPBACK
		public const int LWIP_NETIF_LOOPBACK = 1;
#else
		public const int LWIP_NETIF_LOOPBACK = 0;
#endif

		/**
		 * LWIP_LOOPBACK_MAX_PBUFS: Maximum number of pbufs on queue for loopback
		 * sending for each netif (0 = disabled)
		 */
#if LWIP_LOOPBACK_MAX_PBUFS
		public const int LWIP_LOOPBACK_MAX_PBUFS = 1;
#else
		public const int LWIP_LOOPBACK_MAX_PBUFS = 0;
#endif

		/**
		 * LWIP_NETIF_LOOPBACK_MULTITHREADING: Indicates whether threading is enabled in
		 * the system, as netifs must change how they behave depending on this setting
		 * for the LWIP_NETIF_LOOPBACK option to work.
		 * Setting this is needed to avoid reentering non-reentrant functions like
		 * tcp_input().
		 *    LWIP_NETIF_LOOPBACK_MULTITHREADING==1: Indicates that the user is using a
		 *       multithreaded environment like tcpip.c. In this case, netif.input()
		 *       is called directly.
		 *    LWIP_NETIF_LOOPBACK_MULTITHREADING==0: Indicates a polling (or NO_SYS) setup.
		 *       The packets are put on a list and netif_poll() must be called in
		 *       the main application loop.
		 */
		public const bool LWIP_NETIF_LOOPBACK_MULTITHREADING = NO_SYS == 0;

		/**
		 * LWIP_NETIF_TX_SINGLE_PBUF: if this is set to 1, lwIP tries to put all data
		 * to be sent into one single pbuf. This is for compatibility with DMA-enabled
		 * MACs that do not support scatter-gather.
		 * Beware that this might involve CPU-memcpy before transmitting that would not
		 * be needed without this flag! Use this only if you need to!
		 *
		 * @todo: TCP and IP-frag do not work with this, yet:
		 */
#if LWIP_NETIF_TX_SINGLE_PBUF
		public const int LWIP_NETIF_TX_SINGLE_PBUF = 1;
#else
		public const int LWIP_NETIF_TX_SINGLE_PBUF = 0;
#endif

		/*
		   ------------------------------------
		   ---------- LOOPIF options ----------
		   ------------------------------------
		*/
		/**
		 * LWIP_HAVE_LOOPIF==1: Support loop interface (127.0.0.1) and loopif.c
		 */
#if LWIP_HAVE_LOOPIF
		public const int LWIP_HAVE_LOOPIF = 1;
#else
		public const int LWIP_HAVE_LOOPIF = 0;
#endif

		/*
		   ------------------------------------
		   ---------- SLIPIF options ----------
		   ------------------------------------
		*/
		/**
		 * LWIP_HAVE_SLIPIF==1: Support slip interface and slipif.c
		 */
#if LWIP_HAVE_SLIPIF
		public const int LWIP_HAVE_SLIPIF = 1;
#else
		public const int LWIP_HAVE_SLIPIF = 0;
#endif

		/*
		   ------------------------------------
		   ---------- Thread options ----------
		   ------------------------------------
		*/
		/**
		 * TCPIP_THREAD_NAME: The name assigned to the main tcpip thread.
		 */
		public const string TCPIP_THREAD_NAME = "tcpip_thread";

		/**
		 * TCPIP_THREAD_STACKSIZE: The stack size used by the main tcpip thread.
		 * The stack size value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int TCPIP_THREAD_STACKSIZE = 0;

		/**
		 * TCPIP_THREAD_PRIO: The priority assigned to the main tcpip thread.
		 * The priority value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int TCPIP_THREAD_PRIO = 1;

		/**
		 * TCPIP_MBOX_SIZE: The mailbox size for the tcpip thread messages
		 * The queue size value itself is platform-dependent, but is passed to
		 * sys_mbox_new() when tcpip_init is called.
		 */
		public const int TCPIP_MBOX_SIZE = 0;

		/**
		 * SLIPIF_THREAD_NAME: The name assigned to the slipif_loop thread.
		 */
		public const string SLIPIF_THREAD_NAME = "slipif_loop";

		/**
		 * SLIP_THREAD_STACKSIZE: The stack size used by the slipif_loop thread.
		 * The stack size value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int SLIPIF_THREAD_STACKSIZE = 0;

		/**
		 * SLIPIF_THREAD_PRIO: The priority assigned to the slipif_loop thread.
		 * The priority value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int SLIPIF_THREAD_PRIO = 1;

		/**
		 * PPP_THREAD_NAME: The name assigned to the pppInputThread.
		 */
		public const string PPP_THREAD_NAME = "pppInputThread";

		/**
		 * PPP_THREAD_STACKSIZE: The stack size used by the pppInputThread.
		 * The stack size value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int PPP_THREAD_STACKSIZE = 0;

		/**
		 * PPP_THREAD_PRIO: The priority assigned to the pppInputThread.
		 * The priority value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int PPP_THREAD_PRIO = 1;

		/**
		 * DEFAULT_THREAD_NAME: The name assigned to any other lwIP thread.
		 */
		public const string DEFAULT_THREAD_NAME = "lwIP";

		/**
		 * DEFAULT_THREAD_STACKSIZE: The stack size used by any other lwIP thread.
		 * The stack size value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int DEFAULT_THREAD_STACKSIZE = 0;

		/**
		 * DEFAULT_THREAD_PRIO: The priority assigned to any other lwIP thread.
		 * The priority value itself is platform-dependent, but is passed to
		 * sys_thread_new() when the thread is created.
		 */
		public const int DEFAULT_THREAD_PRIO = 1;

		/**
		 * DEFAULT_RAW_RECVMBOX_SIZE: The mailbox size for the incoming packets on a
		 * NETCONN_RAW. The queue size value itself is platform-dependent, but is passed
		 * to sys_mbox_new() when the recvmbox is created.
		 */
		public const int DEFAULT_RAW_RECVMBOX_SIZE = 0;

		/**
		 * DEFAULT_UDP_RECVMBOX_SIZE: The mailbox size for the incoming packets on a
		 * NETCONN_UDP. The queue size value itself is platform-dependent, but is passed
		 * to sys_mbox_new() when the recvmbox is created.
		 */
		public const int DEFAULT_UDP_RECVMBOX_SIZE = 0;

		/**
		 * DEFAULT_TCP_RECVMBOX_SIZE: The mailbox size for the incoming packets on a
		 * NETCONN_TCP. The queue size value itself is platform-dependent, but is passed
		 * to sys_mbox_new() when the recvmbox is created.
		 */
		public const int DEFAULT_TCP_RECVMBOX_SIZE = 0;

		/**
		 * DEFAULT_ACCEPTMBOX_SIZE: The mailbox size for the incoming connections.
		 * The queue size value itself is platform-dependent, but is passed to
		 * sys_mbox_new() when the acceptmbox is created.
		 */
		public const int DEFAULT_ACCEPTMBOX_SIZE = 0;

		/*
		   ----------------------------------------------
		   ---------- Sequential layer options ----------
		   ----------------------------------------------
		*/
		/**
		 * LWIP_TCPIP_CORE_LOCKING: (EXPERIMENTAL!)
		 * Don't use it if you're not an active lwIP project member
		 */
#if LWIP_TCPIP_CORE_LOCKING
		public const int LWIP_TCPIP_CORE_LOCKING = 1;
#else
		public const int LWIP_TCPIP_CORE_LOCKING = 0;
#endif

		/**
		 * LWIP_TCPIP_CORE_LOCKING_INPUT: (EXPERIMENTAL!)
		 * Don't use it if you're not an active lwIP project member
		 */
#if LWIP_TCPIP_CORE_LOCKING_INPUT
		public const int LWIP_TCPIP_CORE_LOCKING_INPUT = 1;
#else
		public const int LWIP_TCPIP_CORE_LOCKING_INPUT = 0;
#endif

		/**
		 * LWIP_NETCONN==1: Enable Netconn API (require to use api_lib.c)
		 */
#if LWIP_NETCONN
		public const int LWIP_NETCONN = 1;
#else
		public const int LWIP_NETCONN = 0;
#endif

		/** LWIP_TCPIP_TIMEOUT==1: Enable tcpip_timeout/tcpip_untimeout tod create
		 * timers running in tcpip_thread from another thread.
		 */
#if LWIP_TCPIP_TIMEOUT
		public const int LWIP_TCPIP_TIMEOUT = 1;
#else
		public const int LWIP_TCPIP_TIMEOUT = 0;
#endif

		/*
		   ------------------------------------
		   ---------- Socket options ----------
		   ------------------------------------
		*/
		/**
		 * LWIP_SOCKET==1: Enable Socket API (require to use sockets.c)
		 */
#if LWIP_SOCKET
		public const int LWIP_SOCKET = 1;
#else
		public const int LWIP_SOCKET = 0;
#endif

		/**
		 * LWIP_COMPAT_SOCKETS==1: Enable BSD-style sockets functions names.
		 * (only used if you use sockets.c)
		 */
#if LWIP_COMPAT_SOCKETS
		public const int LWIP_COMPAT_SOCKETS = 1;
#else
		public const int LWIP_COMPAT_SOCKETS = 0;
#endif

		/**
		 * LWIP_POSIX_SOCKETS_IO_NAMES==1: Enable POSIX-style sockets functions names.
		 * Disable this option if you use a POSIX operating system that uses the same
		 * names (read, write & close). (only used if you use sockets.c)
		 */
#if LWIP_POSIX_SOCKETS_IO_NAMES
		public const int LWIP_POSIX_SOCKETS_IO_NAMES = 1;
#else
		public const int LWIP_POSIX_SOCKETS_IO_NAMES = 0;
#endif

		/**
		 * LWIP_TCP_KEEPALIVE==1: Enable TCP_KEEPIDLE, TCP_KEEPINTVL and TCP_KEEPCNT
		 * options processing. Note that TCP_KEEPIDLE and TCP_KEEPINTVL have to be set
		 * in seconds. (does not require sockets.c, and will affect tcp.c)
		 */
#if LWIP_TCP_KEEPALIVE
		public const int LWIP_TCP_KEEPALIVE = 1;
#else
		public const int LWIP_TCP_KEEPALIVE = 0;
#endif

		/**
		 * LWIP_SO_SNDTIMEO==1: Enable send timeout for sockets/netconns and
		 * SO_SNDTIMEO processing.
		 */
#if LWIP_SO_SNDTIMEO
		public const int LWIP_SO_SNDTIMEO = 1;
#else
		public const int LWIP_SO_SNDTIMEO = 0;
#endif

		/**
		 * LWIP_SO_RCVTIMEO==1: Enable receive timeout for sockets/netconns and
		 * SO_RCVTIMEO processing.
		 */
#if LWIP_SO_RCVTIMEO
		public const int LWIP_SO_RCVTIMEO = 1;
#else
		public const int LWIP_SO_RCVTIMEO = 0;
#endif

		/**
		 * LWIP_SO_RCVBUF==1: Enable SO_RCVBUF processing.
		 */
#if LWIP_SO_RCVBUF
		public const int LWIP_SO_RCVBUF = 1;
#else
		public const int LWIP_SO_RCVBUF = 0;
#endif

		/**
		 * If LWIP_SO_RCVBUF is used, this is the default value for recv_bufsize.
		 */
		public const int RECV_BUFSIZE_DEFAULT = Int32.MaxValue;

		/**
		 * SO_REUSE==1: Enable SO_REUSEADDR option.
		 */
#if SO_REUSE
		public const int SO_REUSE = 1;
#else
		public const int SO_REUSE = 0;
#endif

		/**
		 * SO_REUSE_RXTOALL==1: Pass a copy of incoming broadcast/multicast packets
		 * to all local matches if SO_REUSEADDR is turned on.
		 * WARNING: Adds a memcpy for every packet if passing to more than one pcb!
		 */
#if SO_REUSE_RXTOALL
		public const int SO_REUSE_RXTOALL = 1;
#else
		public const int SO_REUSE_RXTOALL = 0;
#endif

		/*
		   ----------------------------------------
		   ---------- Statistics options ----------
		   ----------------------------------------
		*/
		/**
		 * LWIP_STATS==1: Enable statistics collection in lwip_stats.
		 */
#if LWIP_STATS
		public const int LWIP_STATS = 1;
#else
		public const int LWIP_STATS = 0;
#endif

#if LWIP_STATS

		/**
 * LWIP_STATS_DISPLAY==1: Compile in the statistics output functions.
 */
#if LWIP_STATS_DISPLAY
		public const int LWIP_STATS_DISPLAY = 1;
#else
		public const int LWIP_STATS_DISPLAY = 0;
#endif

		/**
		 * LINK_STATS==1: Enable link stats.
		 */
#if LINK_STATS
		public const int LINK_STATS = 1;
#else
		public const int LINK_STATS = 0;
#endif

		/**
		 * ETHARP_STATS==1: Enable etharp stats.
		 */
		public const int ETHARP_STATS = (LWIP_ARP);

		/**
		 * IP_STATS==1: Enable IP stats.
		 */
#if IP_STATS
		public const int IP_STATS = 1;
#else
		public const int IP_STATS = 0;
#endif

		/**
		 * IPFRAG_STATS==1: Enable IP fragmentation stats. Default is
		 * on if using either frag or reass.
		 */
		public const bool IPFRAG_STATS = (IP_REASSEMBLY != 0) || (IP_FRAG != 0);

		/**
		 * ICMP_STATS==1: Enable ICMP stats.
		 */
#if ICMP_STATS
		public const int ICMP_STATS = 1;
#else
		public const int ICMP_STATS = 0;
#endif

		/**
		 * IGMP_STATS==1: Enable IGMP stats.
		 */
		public const int IGMP_STATS = (LWIP_IGMP);

		/**
		 * UDP_STATS==1: Enable UDP stats. Default is on if
		 * UDP enabled, otherwise off.
		 */
		public const int UDP_STATS = (LWIP_UDP);

		/**
		 * TCP_STATS==1: Enable TCP stats. Default is on if TCP
		 * enabled, otherwise off.
		 */
		public const int TCP_STATS = (LWIP_TCP);

		/**
		 * MEM_STATS==1: Enable mem.c stats.
		 */
		public const bool MEM_STATS = ((MEM_LIBC_MALLOC == 0) && (MEM_USE_POOLS == 0));

		/**
		 * MEMP_STATS==1: Enable memp.c pool stats.
		 */
		public const bool MEMP_STATS = (MEMP_MEM_MALLOC == 0);

		/**
		 * sys.SYS_STATS==1: Enable system stats (sem and mbox counts, etc).
		 */
		public const bool SYS_STATS = (NO_SYS == 0);

#else

		public const int LINK_STATS = 0;
		public const int IP_STATS = 0;
		public const int IPFRAG_STATS = 0;
		public const int ICMP_STATS = 0;
		public const int IGMP_STATS = 0;
		public const int UDP_STATS = 0;
		public const int TCP_STATS = 0;
		public const int MEM_STATS = 0;
		public const int MEMP_STATS = 0;
		public const int SYS_STATS = 0;
		public const int LWIP_STATS_DISPLAY = 0;

#endif // LWIP_STATS

		/*
		   ---------------------------------
		   ---------- PPP options ----------
		   ---------------------------------
		*/
		/**
		 * PPP_SUPPORT==1: Enable PPP.
		 */
#if PPP_SUPPORT
		public const int PPP_SUPPORT = 1;
#else
		public const int PPP_SUPPORT = 0;
#endif

		/**
		 * PPPOE_SUPPORT==1: Enable PPP Over Ethernet
		 */
#if PPPOE_SUPPORT
		public const int PPPOE_SUPPORT = 1;
#else
		public const int PPPOE_SUPPORT = 0;
#endif

		/**
		 * PPPOS_SUPPORT==1: Enable PPP Over Serial
		 */
		public const int PPPOS_SUPPORT = PPP_SUPPORT;

#if PPP_SUPPORT

		/**
		 * NUM_PPP: Max PPP sessions.
		 */
#if NUM_PPP
		public const int NUM_PPP = 1;
#else
		public const int NUM_PPP = 0;
#endif

		/**
		 * PAP_SUPPORT==1: Support PAP.
		 */
#if PAP_SUPPORT
		public const int PAP_SUPPORT = 1;
#else
		public const int PAP_SUPPORT = 0;
#endif

		/**
		 * CHAP_SUPPORT==1: Support CHAP.
		 */
#if CHAP_SUPPORT
		public const int CHAP_SUPPORT = 1;
#else
		public const int CHAP_SUPPORT = 0;
#endif

		/**
		 * MSCHAP_SUPPORT==1: Support MSCHAP. CURRENTLY NOT SUPPORTED! DO NOT SET!
		 */
#if MSCHAP_SUPPORT
		public const int MSCHAP_SUPPORT = 1;
#else
		public const int MSCHAP_SUPPORT = 0;
#endif

		/**
		 * CBCP_SUPPORT==1: Support CBCP. CURRENTLY NOT SUPPORTED! DO NOT SET!
		 */
#if CBCP_SUPPORT
		public const int CBCP_SUPPORT = 1;
#else
		public const int CBCP_SUPPORT = 0;
#endif

		/**
		 * CCP_SUPPORT==1: Support CCP. CURRENTLY NOT SUPPORTED! DO NOT SET!
		 */
#if CCP_SUPPORT
		public const int CCP_SUPPORT = 1;
#else
		public const int CCP_SUPPORT = 0;
#endif

		/**
		 * VJ_SUPPORT==1: Support VJ header compression.
		 */
#if VJ_SUPPORT
		public const int VJ_SUPPORT = 1;
#else
		public const int VJ_SUPPORT = 0;
#endif

		/**
		 * MD5_SUPPORT==1: Support MD5 (see also CHAP).
		 */
#if MD
		public const int MD5_SUPPORT = 1;
#else
		public const int MD5_SUPPORT = 0;
#endif

		/*
		 * Timeouts
		 */
		public const int FSM_DEFTIMEOUT = 6;
		/* Timeout time in seconds */

		public const int FSM_DEFMAXTERMREQS = 2;
		/* Maximum Terminate-Request transmissions */

		public const int FSM_DEFMAXCONFREQS = 10;
		/* Maximum Configure-Request transmissions */

		public const int FSM_DEFMAXNAKLOOPS = 5;
		/* Maximum number of nak loops */

		public const int UPAP_DEFTIMEOUT = 6;
		/* Timeout (seconds) for retransmitting req */

		public const int UPAP_DEFREQTIME = 30;
		/* Time to wait for auth-req from peer */

		public const int CHAP_DEFTIMEOUT = 6;
		/* Timeout time in seconds */

		public const int CHAP_DEFTRANSMITS = 10;
		/* max # times to send challenge */

		/* Interval in seconds between keepalive echo requests, 0 to disable. */
		public const int LCP_ECHOINTERVAL = 0;

		/* Number of unanswered echo requests before failure. */
		public const int LCP_MAXECHOFAILS = 3;

		/* Max Xmit idle time (in jiffies) before resend flag char. */
		public const int PPP_MAXIDLEFLAG = 100;

		/*
		 * Packet sizes
		 *
		 * Note - lcp shouldn't be allowed to negotiate stuff outside these
		 *    limits.  See lcp.h in the pppd directory.
		 * (XXX - these constants should simply be shared by lcp.c instead
		 *    of living in lcp.h)
		 */
		public const int PPP_MTU = 1500;
		/* Default MTU (size of Info field) */
		/* public const int PPP_MAXMTU = 65535 - (PPP_HDRLEN + PPP_FCSLEN); */
		public const int PPP_MAXMTU = 1500;
		/* Largest MTU we allow */
		public const int PPP_MINMTU = 64;
		public const int PPP_MRU = 1500;
		/* default MRU = max length of info field */
		public const int PPP_MAXMRU = 1500;
		/* Largest MRU we allow */
		public const int PPP_DEFMRU = 296;
		/* Try for this */
		public const int PPP_MINMRU = 128;
		/* No MRUs below this */

		public const int MAXNAMELEN = 256;
		/* max length of hostname or name for auth */
		public const int MAXSECRETLEN = 256;
		/* max length of password or secret */

#endif // PPP_SUPPORT

		/*
		   --------------------------------------
		   ---------- Checksum options ----------
		   --------------------------------------
		*/
		/**
		 * CHECKSUM_GEN_IP==1: Generate checksums in software for outgoing IP packets.
		 */
#if CHECKSUM_GEN_IP
		public const int CHECKSUM_GEN_IP = 1;
#else
		public const int CHECKSUM_GEN_IP = 0;
#endif

		/**
		 * CHECKSUM_GEN_UDP==1: Generate checksums in software for outgoing UDP packets.
		 */
#if CHECKSUM_GEN_UDP
		public const int CHECKSUM_GEN_UDP = 1;
#else
		public const int CHECKSUM_GEN_UDP = 0;
#endif

		/**
		 * CHECKSUM_GEN_TCP==1: Generate checksums in software for outgoing TCP packets.
		 */
#if CHECKSUM_GEN_TCP
		public const int CHECKSUM_GEN_TCP = 1;
#else
		public const int CHECKSUM_GEN_TCP = 0;
#endif

		/**
		 * CHECKSUM_GEN_ICMP==1: Generate checksums in software for outgoing ICMP packets.
		 */
#if CHECKSUM_GEN_ICMP
		public const int CHECKSUM_GEN_ICMP = 1;
#else
		public const int CHECKSUM_GEN_ICMP = 0;
#endif

		/**
		 * CHECKSUM_CHECK_IP==1: Check checksums in software for incoming IP packets.
		 */
#if CHECKSUM_CHECK_IP
		public const int CHECKSUM_CHECK_IP = 1;
#else
		public const int CHECKSUM_CHECK_IP = 0;
#endif

		/**
		 * CHECKSUM_CHECK_UDP==1: Check checksums in software for incoming UDP packets.
		 */
#if CHECKSUM_CHECK_UDP
		public const int CHECKSUM_CHECK_UDP = 1;
#else
		public const int CHECKSUM_CHECK_UDP = 0;
#endif

		/**
		 * CHECKSUM_CHECK_TCP==1: Check checksums in software for incoming TCP packets.
		 */
#if CHECKSUM_CHECK_TCP
		public const int CHECKSUM_CHECK_TCP = 1;
#else
		public const int CHECKSUM_CHECK_TCP = 0;
#endif

		/**
		 * LWIP_CHECKSUM_ON_COPY==1: Calculate checksum when copying data from
		 * application buffers to pbufs.
		 */
#if LWIP_CHECKSUM_ON_COPY
		public const int LWIP_CHECKSUM_ON_COPY = 1;
#else
		public const int LWIP_CHECKSUM_ON_COPY = 0;
#endif

		/*
		   ---------------------------------------
		   ---------- Hook options ---------------
		   ---------------------------------------
		*/

		/* Hooks are undefined by default, define them to a function if you need them. */

		/**
		 * LWIP_HOOK_IP4_INPUT(pbuf, input_netif):
		 * - called from ip_input() (IPv4)
		 * - pbuf: received pbuf passed to ip_input()
		 * - input_netif: netif on which the packet has been received
		 * Return values:
		 * - 0: Hook has not consumed the packet, packet is processed as normal
		 * - != 0: Hook has consumed the packet.
		 * If the hook consumed the packet, 'pbuf' is in the responsibility of the hook
		 * (i.e. free it when done).
		 */

		/**
		 * LWIP_HOOK_IP4_ROUTE(dest):
		 * - called from ip.ip_route() (IPv4)
		 * - dest: destination IPv4 address
		 * Returns the destination netif or null if no destination netif is found. In
		 * that case, ip.ip_route() continues as normal.
		 */

		/*
		   ---------------------------------------
		   ---------- Debugging options ----------
		   ---------------------------------------
		*/
		/**
		 * LWIP_DBG_MIN_LEVEL: After masking, the value of the lwip is
		 * compared against this value. If it is smaller, then debugging
		 * messages are written.
		 */
		public const int LWIP_DBG_MIN_LEVEL = lwip.LWIP_DBG_LEVEL_ALL;

		/**
		 * LWIP_DBG_TYPES_ON: A mask that can be used to globally enable/disable
		 * lwip messages of certain types.
		 */
#if LWIP_DBG_TYPES_ON
		public const uint LWIP_DBG_TYPES_ON = lwip.LWIP_DBG_ON;
#else
		public const uint LWIP_DBG_TYPES_ON = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * ETHARP_DEBUG: Enable debugging in etharp.c.
		 */
#if ETHARP_DEBUG
		public const uint ETHARP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint ETHARP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * NETIF_DEBUG: Enable debugging in netif.c.
		 */
#if NETIF_DEBUG
		public const uint NETIF_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint NETIF_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * PBUF_DEBUG: Enable debugging in pbuf.c.
		 */
#if PBUF_DEBUG
		public const uint PBUF_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint PBUF_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * API_LIB_DEBUG: Enable debugging in api_lib.c.
		 */
#if API_LIB_DEBUG
		public const uint API_LIB_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint API_LIB_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * API_MSG_DEBUG: Enable debugging in api_msg.c.
		 */
#if API_MSG_DEBUG
		public const uint API_MSG_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint API_MSG_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * SOCKETS_DEBUG: Enable debugging in sockets.c.
		 */
#if SOCKETS_DEBUG
		public const uint SOCKETS_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint SOCKETS_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * ICMP_DEBUG: Enable debugging in icmp.c.
		 */
#if ICMP_DEBUG
		public const uint ICMP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint ICMP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * IGMP_DEBUG: Enable debugging in igmp.c.
		 */
#if IGMP_DEBUG
		public const uint IGMP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint IGMP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * INET_DEBUG: Enable debugging in inet.c.
		 */
#if INET_DEBUG
		public const uint INET_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint INET_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * IP_DEBUG: Enable debugging for IP.
		 */
#if IP_DEBUG
		public const uint IP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint IP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * IP_REASS_DEBUG: Enable debugging in ip_frag.c for both frag & reass.
		 */
#if IP_REASS_DEBUG
		public const uint IP_REASS_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint IP_REASS_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * RAW_DEBUG: Enable debugging in raw.c.
		 */
#if RAW_DEBUG
		public const uint RAW_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint RAW_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * MEM_DEBUG: Enable debugging in mem.c.
		 */
#if MEM_DEBUG
		public const uint MEM_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint MEM_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * MEMP_DEBUG: Enable debugging in memp.c.
		 */
#if MEMP_DEBUG
		public const uint MEMP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint MEMP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * sys.SYS_DEBUG: Enable debugging in sys.c.
		 */
#if SYS_DEBUG
		public const uint SYS_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint SYS_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TIMERS_DEBUG: Enable debugging in timers.c.
		 */
#if TIMERS_DEBUG
		public const uint TIMERS_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TIMERS_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_DEBUG: Enable debugging for TCP.
		 */
#if TCP_DEBUG
		public const uint TCP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_INPUT_DEBUG: Enable debugging in tcp_in.c for incoming lwip.
		 */
#if TCP_INPUT_DEBUG
		public const uint TCP_INPUT_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_INPUT_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_FR_DEBUG: Enable debugging in tcp_in.c for fast retransmit.
		 */
#if TCP_FR_DEBUG
		public const uint TCP_FR_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_FR_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_RTO_DEBUG: Enable debugging in TCP for retransmit
		 * timeout.
		 */
#if TCP_RTO_DEBUG
		public const uint TCP_RTO_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_RTO_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_CWND_DEBUG: Enable debugging for TCP congestion window.
		 */
#if TCP_CWND_DEBUG
		public const uint TCP_CWND_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_CWND_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_WND_DEBUG: Enable debugging in tcp_in.c for window updating.
		 */
#if TCP_WND_DEBUG
		public const uint TCP_WND_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_WND_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_OUTPUT_DEBUG: Enable debugging in tcp_out.c output functions.
		 */
#if TCP_OUTPUT_DEBUG
		public const uint TCP_OUTPUT_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_OUTPUT_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_RST_DEBUG: Enable debugging for TCP with the RST message.
		 */
#if TCP_RST_DEBUG
		public const uint TCP_RST_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_RST_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCP_QLEN_DEBUG: Enable debugging for TCP queue lengths.
		 */
#if TCP_QLEN_DEBUG
		public const uint TCP_QLEN_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCP_QLEN_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * UDP_DEBUG: Enable debugging in UDP.
		 */
#if UDP_DEBUG
		public const uint UDP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint UDP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * TCPIP_DEBUG: Enable debugging in tcpip.c.
		 */
#if TCPIP_DEBUG
		public const uint TCPIP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint TCPIP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * PPP_DEBUG: Enable debugging for PPP.
		 */
#if PPP_DEBUG
		public const uint PPP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint PPP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * SLIP_DEBUG: Enable debugging in slipif.c.
		 */
#if SLIP_DEBUG
		public const uint SLIP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint SLIP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * DHCP_DEBUG: Enable debugging in dhcp.c.
		 */
#if DHCP_DEBUG
		public const uint DHCP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint DHCP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * AUTOIP_DEBUG: Enable debugging in autoip.c.
		 */
#if AUTOIP_DEBUG
		public const uint AUTOIP_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint AUTOIP_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * SNMP_MSG_DEBUG: Enable debugging for SNMP messages.
		 */
#if SNMP_MSG_DEBUG
		public const uint SNMP_MSG_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint SNMP_MSG_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * SNMP_MIB_DEBUG: Enable debugging for SNMP MIBs.
		 */
#if SNMP_MIB_DEBUG
		public const uint SNMP_MIB_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint SNMP_MIB_DEBUG = lwip.LWIP_DBG_OFF;
#endif

		/**
		 * DNS_DEBUG: Enable debugging for DNS.
		 */
#if DNS_DEBUG
		public const uint DNS_DEBUG = lwip.LWIP_DBG_ON;
#else
		public const uint DNS_DEBUG = lwip.LWIP_DBG_OFF;
#endif
	}
}
