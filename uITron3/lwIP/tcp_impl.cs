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
	public partial class tcp
	{
		/**
		 * This is the Nagle algorithm: try to combine user data to send as few TCP
		 * segments as possible. Only send if
		 * - no previously transmitted data on the connection remains unacknowledged or
		 * - the tcp_pcb.TF_NODELAY flag is set (nagle algorithm turned off for this pcb) or
		 * - the only unsent segment is at least pcb.mss bytes long (or there is more
		 *   than one unsent segment - with lwIP, this can happen although unsent.len < mss)
		 * - or if we are in fast-retransmit (tcp_pcb.TF_INFR)
		 */
		public static bool tcp_do_output_nagle(tcp_pcb tpcb)
		{
			return ((tpcb.unacked == null) ||
				((tpcb.flags & (tcp_pcb.TF_NODELAY | tcp_pcb.TF_INFR)) != 0) ||
				((tpcb.unsent != null) && ((tpcb.unsent.next != null) ||
				(tpcb.unsent.len >= tpcb.mss))) /*||
				((tcp_sndbuftpcb == 0) || (tcp_sndqueuelentpcb >= opt.TCP_SND_QUEUELEN))*/
				);
		}
		public err_t tcp_output_nagle(tcp_pcb tpcb) { return tcp_do_output_nagle(tpcb) ? tcp_output(tpcb) : err_t.ERR_OK; }

		public static bool TCP_SEQ_LT(uint a, uint b) { return ((int)(a - b) < 0); }
		public static bool TCP_SEQ_LEQ(uint a, uint b) { return ((int)(a - b) <= 0); }
		public static bool TCP_SEQ_GT(uint a, uint b) { return ((int)(a - b) > 0); }
		public static bool TCP_SEQ_GEQ(uint a, uint b) { return ((int)(a - b) >= 0); }
		/* is b<=a<=c? */
		public static bool TCP_SEQ_BETWEEN(uint a, uint b, uint c)
		{
#if false // see bug #10548
			return ((c)-(b) >= (a)-(b));
#endif
			return (TCP_SEQ_GEQ(a, b) && TCP_SEQ_LEQ(a, c));
		}
		public const byte TCP_FIN = (byte)0x01U;
		public const byte TCP_SYN = (byte)0x02U;
		public const byte TCP_RST = (byte)0x04U;
		public const byte TCP_PSH = (byte)0x08U;
		public const byte TCP_ACK = (byte)0x10U;
		public const byte TCP_URG = (byte)0x20U;
		public const byte TCP_ECE = (byte)0x40U;
		public const byte TCP_CWR = (byte)0x80U;

		public const byte TCP_FLAGS = (byte)0x3fU;

		/* Length of the TCP header, excluding options. */
		public const int TCP_HLEN = 20;

		public const int TCP_TMR_INTERVAL = 250;  /* The TCP timer interval in milliseconds. */

		public const int TCP_FAST_INTERVAL = TCP_TMR_INTERVAL; /* the fine grained timeout in milliseconds */

		public const int TCP_SLOW_INTERVAL = (2 * TCP_TMR_INTERVAL);  /* the coarse grained timeout in milliseconds */

		public const int TCP_FIN_WAIT_TIMEOUT = 20000; /* milliseconds */
		public const int TCP_SYN_RCVD_TIMEOUT = 20000; /* milliseconds */

		public const uint TCP_OOSEQ_TIMEOUT = 6U; /* x RTO */

		public const uint TCP_MSL = 60000U; /* The maximum segment lifetime in milliseconds */

		/* Keepalive values, compliant with RFC 1122. Don't change this unless you know what you're doing */
		public const uint TCP_KEEPIDLE_DEFAULT = 7200000U; /* Default KEEPALIVE timer in milliseconds */

		public const uint TCP_KEEPINTVL_DEFAULT = 75000U;   /* Default Time between KEEPALIVE probes in milliseconds */

		public const uint TCP_KEEPCNT_DEFAULT = 9U;        /* Default Counter for KEEPALIVE probes */

		public const uint TCP_MAXIDLE = TCP_KEEPCNT_DEFAULT * TCP_KEEPINTVL_DEFAULT;  /* Maximum KEEPALIVE probe time */
	}

	/* Fields are (of course) in network byte order.
	 * Some fields are converted to host byte order in tcp_input().
	 */
	public class tcp_hdr : pointer
	{
		public new const int length = 20;
		pointer _src;
		pointer _dest;
		pointer _seqno;
		pointer _ackno;
		pointer __hdrlen_rsvd_flags;
		pointer _wnd;
		pointer _chksum;
		pointer _urgp;

		public tcp_hdr(byte[] buffer, int offset)
			: base(buffer, offset)
		{
			_src = new pointer(data, offset + 0); /* ushort */
			_dest = new pointer(data, offset + 2); /* ushort */
			_seqno = new pointer(data, offset + 4); /* uint */
			_ackno = new pointer(data, offset + 8); /* uint */
			__hdrlen_rsvd_flags = new pointer(data, offset + 12); /* ushort */
			_wnd = new pointer(data, offset + 14); /* ushort */
			_chksum = new pointer(data, offset + 16); /* ushort */
			_urgp = new pointer(data, offset + 18); /* ushort */
		}

		public tcp_hdr(byte[] buffer)
			: this(buffer, 0)
		{
		}

		public tcp_hdr(pointer buffer)
			: this(buffer.data, buffer.offset)
		{
		}

		public ushort src { get { return (ushort)_src; } set { _src.SetValue(value); } }
		public ushort dest { get { return (ushort)_dest; } set { _dest.SetValue(value); } }
		public uint seqno { get { return (uint)_seqno; } set { _seqno.SetValue(value); } }
		public uint ackno { get { return (uint)_ackno; } set { _ackno.SetValue(value); } }
		public ushort _hdrlen_rsvd_flags { get { return (ushort)__hdrlen_rsvd_flags; } set { __hdrlen_rsvd_flags.SetValue(value); } }
		public ushort wnd { get { return (ushort)_wnd; } set { _wnd.SetValue(value); } }
		public ushort chksum { get { return (ushort)_chksum; } set { _chksum.SetValue(value); } }
		public ushort urgp { get { return (ushort)_urgp; } set { _urgp.SetValue(value); } }

		public static byte TCPH_HDRLEN(tcp_hdr phdr) { return (byte)(lwip.lwip_ntohs(phdr._hdrlen_rsvd_flags) >> 12); }
		public static byte TCPH_FLAGS(tcp_hdr phdr) { return (byte)(lwip.lwip_ntohs((phdr)._hdrlen_rsvd_flags) & tcp.TCP_FLAGS); }

		public static void TCPH_HDRLEN_SET(tcp_hdr phdr, int len) { phdr._hdrlen_rsvd_flags = (ushort)(lwip.lwip_htons((ushort)(((len) << 12) | tcp_hdr.TCPH_FLAGS(phdr)))); }
		public static void TCPH_FLAGS_SET(tcp_hdr phdr, ushort flags) { phdr._hdrlen_rsvd_flags = (ushort)(((phdr)._hdrlen_rsvd_flags & lwip.PP_HTONS(unchecked((ushort)((~(ushort)(tcp.TCP_FLAGS))))) | lwip.lwip_htons(flags))); }
		public static void TCPH_HDRLEN_FLAGS_SET(tcp_hdr phdr, int len, ushort flags) { phdr._hdrlen_rsvd_flags = lwip.lwip_htons((ushort)(((len) << 12) | (flags))); }

		public static void TCPH_SET_FLAG(tcp_hdr phdr, ushort flags) { phdr._hdrlen_rsvd_flags = (ushort)((phdr)._hdrlen_rsvd_flags | lwip.lwip_htons(flags)); }
		public static void TCPH_UNSET_FLAG(tcp_hdr phdr, ushort flags) { phdr._hdrlen_rsvd_flags = (ushort)lwip.lwip_htons((ushort)(lwip.lwip_ntohs((phdr)._hdrlen_rsvd_flags) | (tcp_hdr.TCPH_FLAGS(phdr) & ~(flags)))); }

		public static int TCP_TCPLEN(tcp_seg seg) { return seg.len + (((tcp_hdr.TCPH_FLAGS(seg.tcphdr) & (tcp.TCP_FIN | tcp.TCP_SYN)) != 0) ? 1 : 0); }
	}

	public partial class tcp
	{
		/** Flags used on input processing, not on pcb.flags
		*/
		public const byte TF_RESET = (byte)0x08U;   /* Connection was reset. */
		public const byte TF_CLOSED = (byte)0x10U;  /* Connection was sucessfully closed. */
		public const byte TF_GOT_FIN = (byte)0x20U;   /* Connection was closed by the remote end. */


#if LWIP_EVENT_API

		public void TCP_EVENT_ACCEPT(tcp_pcb pcb, err_t err, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_ACCEPT, null, 0, err);
		}
		public void TCP_EVENT_SENT(tcp_pcb pcb, ushort space, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_SENT, null, space, err_t.ERR_OK);
		}
		public void TCP_EVENT_RECV(tcp_pcb pcb, pbuf p, err_t err, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_RECV, p, 0, err);
		}
		public void TCP_EVENT_CLOSED(tcp_pcb pcb, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_RECV, null, 0, err_t.ERR_OK);
		}
		public void TCP_EVENT_CONNECTED(tcp_pcb pcb, err_t err, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_CONNECTED, null, 0, err);
		}
		public void TCP_EVENT_POLL(tcp_pcb pcb, out err_t ret)
		{
			ret = lwip_tcp_event(pcb.callback_arg, pcb,
				lwip_event.LWIP_EVENT_POLL, null, 0, err_t.ERR_OK);
		}
		public err_t TCP_EVENT_ERR(tcp_err_fn errf, object arg, err_t err)
		{
			return lwip_tcp_event(arg, null,
				lwip_event.LWIP_EVENT_ERR, null, 0, err);
		}

#else // LWIP_EVENT_API

		public static void TCP_EVENT_ACCEPT(tcp_pcb pcb, err_t err, out err_t ret)
		{
			do {
				if (pcb.accept != null)
					ret = pcb.accept(pcb.callback_arg, pcb, err);
				else ret = err_t.ERR_ARG;
			} while (false);
		}

		public static void TCP_EVENT_SENT(tcp_pcb pcb, ushort space, out err_t ret)
		{
			do {
				if (pcb.sent != null)
					ret = pcb.sent(pcb.callback_arg, pcb, space);
				else ret = err_t.ERR_OK;
			} while (false);
		}

		public void TCP_EVENT_RECV(tcp_pcb pcb, pbuf p, err_t err, out err_t ret)
		{
			do {
				if (pcb.recv != null) {
					ret = pcb.recv(pcb.callback_arg, pcb, p, err);
				}
				else {
					ret = tcp_recv_null(null, pcb, p, err);
				}
			} while (false);
		}

		public static void TCP_EVENT_CLOSED(tcp_pcb pcb, out err_t ret)
		{
			do {
				if ((pcb.recv != null)) {
					ret = pcb.recv(pcb.callback_arg, pcb, null, err_t.ERR_OK);
				}
				else {
					ret = err_t.ERR_OK;
				}
			} while (false);
		}

		public static void TCP_EVENT_CONNECTED(tcp_pcb pcb, err_t err, out err_t ret)
		{
			do {
				if (pcb.connected != null)
					ret = pcb.connected(pcb.callback_arg, pcb, err);
				else ret = err_t.ERR_OK;
			} while (false);
		}

		public static void TCP_EVENT_POLL(tcp_pcb pcb, out err_t ret)
		{
			do {
				if (pcb.poll != null)
					ret = pcb.poll(pcb.callback_arg, pcb);
				else ret = err_t.ERR_OK;
			} while (false);
		}

		public static void TCP_EVENT_ERR(tcp_err_fn errf, object arg, err_t err)
		{
			do {
				if (errf != null)
					errf(arg, err);
			} while (false);
		}

#endif // LWIP_EVENT_API

		/** Enabled extra-check for TCP_OVERSIZE if LWIP_DEBUG is enabled */
#if TCP_OVERSIZE && LWIP_DEBUG
		public const int TCP_OVERSIZE_DBGCHECK = 1;
#else
		public const int TCP_OVERSIZE_DBGCHECK = 0;
#endif

		/** Don't generate checksum on copy if CHECKSUM_GEN_TCP is disabled */
		public const bool TCP_CHECKSUM_ON_COPY = (opt.LWIP_CHECKSUM_ON_COPY != 0) && (opt.CHECKSUM_GEN_TCP != 0);
	}
	/* This structure represents a TCP segment on the unsent, unacked and ooseq queues */
	public class tcp_seg : memp
	{
		public tcp_seg(lwip lwip)
			: base(lwip)
		{
		}

		public tcp_seg next;    /* used when putting segements on a queue */
		public pbuf p;          /* buffer containing data + TCP header */
		public ushort len;               /* the TCP length of this segment */
#if TCP_OVERSIZE_DBGCHECK
		public ushort oversize_left;     /* Extra bytes available at the end of the last
											pbuf in unsent (used for asserting vs.
											tcp_pcb.unsent_oversized only) */
#endif // TCP_OVERSIZE_DBGCHECK
#if TCP_CHECKSUM_ON_COPY
		public ushort chksum;
		public bool chksum_swapped;
#endif // TCP_CHECKSUM_ON_COPY
		public byte flags;
		public const byte TF_SEG_OPTS_MSS = (byte)0x01U; /* Include MSS option. */
		public const byte TF_SEG_OPTS_TS = (byte)0x02U; /* Include timestamp option. */
		public const byte TF_SEG_DATA_CHECKSUMMED = (byte)0x04U;    /* ALL data (not the header) is
																	   checksummed into 'chksum' */
		public tcp_hdr tcphdr;  /* the TCP header */

		public static int LWIP_TCP_OPT_LENGTH(ushort flags)
		{
			return
				(((flags & TF_SEG_OPTS_MSS) != 0) ? 4 : 0) +
				(((flags & TF_SEG_OPTS_TS) != 0) ? 12 : 0);
		}

		/** This returns a TCP header option for MSS in an uint */
		public static uint TCP_BUILD_MSS_OPTION(uint mss) { return lwip.lwip_htonl(0x02040000 | ((mss) & 0xFFFF)); }

		internal void copy_from(tcp_seg seg)
		{
			this.next = seg.next;
			this.p = seg.p;
			this.len = seg.len;
#if TCP_OVERSIZE_DBGCHECK
			this.oversize_left = seg.oversize_left;
#endif // TCP_OVERSIZE_DBGCHECK
#if TCP_CHECKSUM_ON_COPY
			this.chksum = seg.chksum;
			this.chksum_swapped = seg.chksum_swapped;
#endif // TCP_CHECKSUM_ON_COPY
			this.flags = seg.flags;
			this.tcphdr = seg.tcphdr;
		}
	}

	/* The TCP PCB lists. */
	public class tcp_listen_pcbs_t
	{ /* List of all TCP PCBs in tcp_state.LISTEN state. */
		public tcp_pcb_common pcbs;
		public tcp_pcb_listen listen_pcbs { get { return (tcp_pcb_listen)pcbs; } set { pcbs = value; } }
	}

	public partial class tcp
	{
		/* Axioms about the above lists:   
		   1) Every TCP PCB that is not tcp_state.CLOSED is in one of the lists.
		   2) A PCB is only in one of the lists.
		   3) All PCBs in the tcp_listen_pcbs list is in tcp_state.LISTEN state.
		   4) All PCBs in the tcp_tw_pcbs list is in TIME-WAIT state.
		*/
		/* Define two macros, TCP_REG and TCP_RMV that registers a TCP PCB
		   with a PCB list or removes a PCB from a list, respectively. */
#if !TCP_DEBUG_PCB_LISTS
		public const int TCP_DEBUG_PCB_LISTS = 0;
#endif
#if TCP_DEBUG_PCB_LISTS
		public void TCP_REG<T>(ref T pcbs, T npcb) where T : tcp_pcb_common
		{
			do {
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "TCP_REG {0} local port {1}\n", (npcb), (npcb).local_port);
				for (tcp_tmp_pcb = (pcbs);
					tcp_tmp_pcb != null;
					tcp_tmp_pcb = tcp_tmp_pcb.next) {
					lwip.LWIP_ASSERT("TCP_REG: already registered\n", tcp_tmp_pcb != (npcb));
				}
				lwip.LWIP_ASSERT("TCP_REG: pcb.state != tcp_state.CLOSED", ((pcbs) == tcp_bound_pcbs) || ((npcb).state != tcp_state.CLOSED));
				(npcb).next = pcbs;
				lwip.LWIP_ASSERT("TCP_REG: npcb.next != npcb", (npcb).next != (npcb));
				pcbs = (npcb);
				lwip.LWIP_ASSERT("TCP_RMV: tcp_pcbs sane", tcp_pcbs_sane() != 0);
				lwip.sys.tcp_timer_needed();
			} while (false);
		}

		public void TCP_RMV<T>(ref T pcbs, T npcb) where T : tcp_pcb_common
		{
			do {
				lwip.LWIP_ASSERT("TCP_RMV: pcbs != null", pcbs != null);
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "TCP_RMV: removing {0} from {1}\n", npcb, pcbs);
				if (pcbs == npcb) {
					pcbs = (T)(pcbs).next;
				}
				else
					for (tcp_tmp_pcb = pcbs; tcp_tmp_pcb != null; tcp_tmp_pcb = tcp_tmp_pcb.next) {
						if (tcp_tmp_pcb.next == npcb) {
							tcp_tmp_pcb.next = npcb.next;
							break;
						}
					}
				npcb.next = null;
				lwip.LWIP_ASSERT("TCP_RMV: tcp_pcbs sane", tcp_pcbs_sane() != 0);
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "TCP_RMV: removed {0} from {1}\n", npcb, pcbs);
			} while (false);
		}

#else // LWIP_DEBUG

		public void TCP_REG<T>(ref T pcbs, T npcb) where T : tcp_pcb_common
		{
			do
			{
				(npcb).next = pcbs;
				pcbs = (npcb);
				lwip.sys.tcp_timer_needed();
			} while (false);
		}

		public void TCP_RMV<T>(ref T pcbs, T npcb) where T : tcp_pcb_common
		{
			do
			{
				if (pcbs == (npcb))
				{
					pcbs = (T)(pcbs).next;
				}
				else
				{
					for (tcp_tmp_pcb = pcbs;
						tcp_tmp_pcb != null;
						tcp_tmp_pcb = tcp_tmp_pcb.next)
					{
						if (tcp_tmp_pcb.next == (npcb))
						{
							tcp_tmp_pcb.next = (npcb).next;
							break;
						}
					}
				}
				(npcb).next = null;
			} while (false);
		}

#endif // LWIP_DEBUG

		public void TCP_REG_ACTIVE(tcp_pcb npcb)
		{
			do {
				TCP_REG(ref tcp_active_pcbs, npcb);
				tcp_active_pcbs_changed = 1;
			} while (false);
		}

		public void TCP_RMV_ACTIVE(tcp_pcb npcb)
		{
			do {
				TCP_RMV(ref tcp_active_pcbs, npcb);
				tcp_active_pcbs_changed = 1;
			} while (false);
		}

		public void TCP_PCB_REMOVE_ACTIVE(tcp_pcb_common pcb)
		{
			do {
				tcp_pcb_remove(tcp_active_pcbs, pcb);
				tcp_active_pcbs_changed = 1;
			} while (false);
		}


		/* Internal functions: */

		public static void tcp_ack(tcp_pcb pcb)
		{
			do {
				if ((pcb.flags & tcp_pcb.TF_ACK_DELAY) != 0) {
					pcb.flags &= unchecked((byte)~tcp_pcb.TF_ACK_DELAY);
					pcb.flags |= tcp_pcb.TF_ACK_NOW;
				}
				else {
					pcb.flags |= tcp_pcb.TF_ACK_DELAY;
				}
			} while (false);
		}

		public static void tcp_ack_now(tcp_pcb pcb)
		{
			do {
				pcb.flags |= tcp_pcb.TF_ACK_NOW;
			} while (false);
		}
	}
}
