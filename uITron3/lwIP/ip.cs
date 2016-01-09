/**
 * @file
 * This is the IPv4 layer implementation for incoming and outgoing IP traffic.
 * 
 * @see ip_frag.c
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
	public class ip_pcb : memp
	{
		public ip_pcb(lwip lwip)
			: base(lwip)
		{
		}

		/* Common members of all PCB types */
		/* ip addresses in network byte order */
		private ip_addr _local_ip = new ip_addr(new byte[ip_addr.length]);
		private ip_addr _remote_ip = new ip_addr(new byte[ip_addr.length]);
		public ip_addr local_ip { get { return _local_ip; } }
		public ip_addr remote_ip { get { return _remote_ip; } }
		/* Socket options */
		public byte so_options;
		/* Type Of Service */
		public byte tos;
		/* Time To Live */
		public byte ttl;
		/* link layer address resolution hint */
#if LWIP_NETIF_HWADDRHINT
		public byte addr_hint;
#endif // LWIP_NETIF_HWADDRHINT
	}

	/*
	 * Option flags per-socket. These are the same like SO_XXX.
	 */
	public static class sof
	{
		/*public const byte SOF_DEBUG     = 0x01;     Unimplemented: turn on debugging info recording */
		public const byte SOF_ACCEPTCONN = 0x02;  /* socket has had listen() */
		public const byte SOF_REUSEADDR = 0x04;  /* allow local address reuse */
		public const byte SOF_KEEPALIVE = 0x08;  /* keep connections alive */
												 /*public const uint SOF_DONTROUTE = 0x10U;     Unimplemented: just use interface addresses */
		public const byte SOF_BROADCAST = 0x20;  /* permit to send and to receive broadcast messages (see IP_SOF_BROADCAST option) */
												 /*public const uint SOF_USELOOPBACK = 0x40U;     Unimplemented: bypass hardware when possible */
		public const byte SOF_LINGER = 0x80;  /* linger on close if data present */
											  /*public const uint SOF_OOBINLINE = 0x0100U;   Unimplemented: leave received OOB data in line */
											  /*public const uint SOF_REUSEPORT = 0x0200U;   Unimplemented: allow local address & port reuse */

		/* These flags are inherited (e.g. from a listen-pcb to a connection-pcb): */
		public const byte SOF_INHERITED = (SOF_REUSEADDR | SOF_KEEPALIVE | SOF_LINGER);/*|SOF_DEBUG|SOF_DONTROUTE|SOF_OOBINLINE*/
	}

	public delegate void netif_output_t(lwip netif, byte[] packet, ip_addr src, ip_addr dest, byte proto);

	public partial class lwip
	{
		public const int IP_PROTO_ICMP = 1;
		public const int IP_PROTO_IGMP = 2;
		public const int IP_PROTO_UDP = 17;
		public const int IP_PROTO_UDPLITE = 136;
		public const int IP_PROTO_TCP = 6;
		public const byte NETIF_FLAG_BROADCAST = (byte)0x02U;

		public ushort mtu = 1500;
		public ip_addr ip_addr = new ip_addr(0x00000000);
		public ip_addr netmask = new ip_addr(0xFFFFFFFF);
		public byte flags = NETIF_FLAG_BROADCAST;
		private netif_output_t m_output;

		public lwip(netif_output_t output)
		{
			m_output = output;
		}

		internal static bool ip_get_option(ip_pcb pcb, byte flag)
		{
			return (pcb.so_options & flag) != 0;
		}

		internal static void ip_set_option(ip_pcb pcb, byte flag)
		{
			pcb.so_options |= flag;
		}

		internal ip_addr current_iphdr_src = new ip_addr(new byte[ip_addr.length]);
		internal ip_addr current_iphdr_dest = new ip_addr(new byte[ip_addr.length]);

		internal ip_addr ip_current_src_addr()
		{
			return current_iphdr_src;
		}

		internal ip_addr ip_current_dest_addr()
		{
			return current_iphdr_dest;
		}

		/**
		 * Simple interface to ip_output_if. It finds the outgoing network
		 * interface and calls upon ip_output_if to do the actual work.
		 *
		 * @param p the packet to send (p.payload points to the data, e.g. next
					protocol header; if dest == ip.IP_HDRINCL, p already includes an IP
					header and p.payload points to that IP header)
		 * @param src the source IP address to send from (if src == IP_ADDR_ANY, the
		 *         IP  address of the netif used to send is used as source address)
		 * @param dest the destination IP address to send the packet to
		 * @param ttl the TTL value to be set in the IP header
		 * @param tos the TOS value to be set in the IP header
		 * @param proto the PROTOCOL to be set in the IP header
		 *
		 * @return err_t.ERR_RTE if no route is found
		 *         see ip_output_if() for more return values
		 */
		public err_t ip_output(pbuf p, ip_addr src, ip_addr dest,
			byte ttl, byte tos, byte proto)
		{
			/* pbufs passed to IP must have a @ref-count of 1 as their payload pointer
			   gets altered as the packet is passed down the stack */
			lwip.LWIP_ASSERT("p.ref == 1", p.@ref == 1);

			return ip_output_if(p, src, dest, ttl, tos, proto);
		}

#if LWIP_NETIF_HWADDRHINT
		/** Like ip.ip_output, but takes and addr_hint pointer that is passed on to netif.addr_hint
		 *  before calling ip_output_if.
		 *
		 * @param p the packet to send (p.payload points to the data, e.g. next
					protocol header; if dest == ip.IP_HDRINCL, p already includes an IP
					header and p.payload points to that IP header)
		 * @param src the source IP address to send from (if src == IP_ADDR_ANY, the
		 *         IP  address of the netif used to send is used as source address)
		 * @param dest the destination IP address to send the packet to
		 * @param ttl the TTL value to be set in the IP header
		 * @param tos the TOS value to be set in the IP header
		 * @param proto the PROTOCOL to be set in the IP header
		 * @param addr_hint address hint pointer set to netif.addr_hint before
		 *        calling ip_output_if()
		 *
		 * @return err_t.ERR_RTE if no route is found
		 *         see ip_output_if() for more return values
		 */
		public err_t ip_output_hinted(pbuf p, ip_addr src, ip_addr dest,
			byte ttl, byte tos, byte proto, object addr_hint)
		{
			err_t err;

			/* pbufs passed to IP must have a @ref-count of 1 as their payload pointer
			   gets altered as the packet is passed down the stack */
			lwip.LWIP_ASSERT("p.ref == 1", p.@ref == 1);

			err = ip_output_if(p, src, dest, ttl, tos, proto);

			return err;
		}
#endif // LWIP_NETIF_HWADDRHINT

		internal err_t ip_output_if(pbuf p, ip_addr src, ip_addr dest, byte ttl, byte tos, byte proto)
		{
			/* pbufs passed to IP must have a @ref-count of 1 as their payload pointer
			 gets altered as the packet is passed down the stack */
			lwip.LWIP_ASSERT("p.ref == 1", p.@ref == 1);

			++lwip_stats.ip.xmit;

			return output(this, p, src, dest, ttl, tos, proto);
		}

		internal err_t ip_input(pbuf p, ip_addr src, ip_addr dest, byte proto)
		{
			++lwip_stats.ip.recv;

			/* copy IP addresses to aligned ip_addr */
			ip_addr.ip_addr_copy(current_iphdr_dest, dest);
			ip_addr.ip_addr_copy(current_iphdr_src, src);

			/* broadcast or multicast packet source address? Compliant with RFC 1122: 3.2.1.3 */
#if IP_ACCEPT_LINK_LAYER_ADDRESSING
			/* DHCP servers need 0.0.0.0 to be allowed as source address (RFC 1.1.2.2: 3.2.1.3/a) */
			if (check_ip_src != 0 && !ip_addr.ip_addr_isany(ip.current_iphdr_src))
#endif // IP_ACCEPT_LINK_LAYER_ADDRESSING
			{
				if ((ip_addr.ip_addr_isbroadcast(current_iphdr_src, this)) ||
					(ip_addr.ip_addr_ismulticast(current_iphdr_src))) {
					/* packet source is not valid */
					lwip.LWIP_DEBUGF(opt.IP_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_LEVEL_WARNING, "ip_input: packet source is not valid.\n");
					/* free (drop) packet pbufs */
					pbuf_free(p);
					++lwip_stats.ip.drop;
					//snmp.snmp_inc_ipinaddrerrors();
					//snmp.snmp_inc_ipindiscards();
					return err_t.ERR_OK;
				}
			}

			/* send to upper layers */
			lwip.LWIP_DEBUGF(opt.IP_DEBUG, "ip_input: \n");
#if IP_DEBUG
			ip_debug_print(p);
#endif
			lwip.LWIP_DEBUGF(opt.IP_DEBUG, "ip_input: p.len {0} p.tot_len {1}\n", p.len, p.tot_len);

			//lwip.current_header = iphdr;

#if LWIP_RAW
			/* raw input did not eat the packet? */
			if (raw.raw_input(p, inp) == 0)
#endif // LWIP_RAW
			{
				switch (proto) {
#if LWIP_UDP
				case lwip.IP_PROTO_UDP:
#if LWIP_UDPLITE
				case lwip.IP_PROTO_UDPLITE:
#endif // LWIP_UDPLITE
					//snmp.snmp_inc_ipindelivers();
					udp.udp_input(p, this, proto == lwip.IP_PROTO_UDPLITE);
					break;
#endif // LWIP_UDP
#if LWIP_TCP
				case lwip.IP_PROTO_TCP:
					//snmp.snmp_inc_ipindelivers();
					tcp.tcp_input(p, this);
					break;
#endif // LWIP_TCP
#if LWIP_ICMP
				case lwip.IP_PROTO_ICMP:
					//snmp.snmp_inc_ipindelivers();
					lwip.icmp.icmp_input(p, inp);
					break;
#endif // LWIP_ICMP
#if LWIP_IGMP
				case lwip.IP_PROTO_IGMP:
					lwip.igmp.igmp_input(p, inp, lwip.current_iphdr_dest);
					break;
#endif // LWIP_IGMP
				default:
#if LWIP_ICMP
					/* send ICMP destination protocol unreachable unless is was a broadcast */
					if (!ip_addr.ip_addr_isbroadcast(lwip.current_iphdr_dest, inp) &&
						!ip_addr.ip_addr_ismulticast(lwip.current_iphdr_dest))
					{
						p.payload = iphdr;
						lwip.icmp.icmp_dest_unreach(p, icmp_dur_type.ICMP_DUR_PROTO);
					}
#endif // LWIP_ICMP
					pbuf_free(p);

					LWIP_DEBUGF(opt.IP_DEBUG | LWIP_DBG_LEVEL_SERIOUS, "Unsupported transport protocol {0}\n", proto);

					++lwip_stats.ip.proterr;
					++lwip_stats.ip.drop;
					//snmp.snmp_inc_ipinunknownprotos();
					break;
				}
			}

			//lwip.current_header = null;
			ip_addr.ip_addr_set_any(current_iphdr_src);
			ip_addr.ip_addr_set_any(current_iphdr_dest);

			return err_t.ERR_OK;
		}

		internal err_t output(lwip ip, pbuf p, ip_addr src, ip_addr dest, byte ttl, byte tos, byte proto)
		{
			int pos = 0, rest = p.tot_len;
			byte[] packet = new byte[rest];
			ip_addr srch = new ip_addr(lwip_ntohl(src.addr));
			ip_addr desth = new ip_addr(lwip_ntohl(dest.addr));

			for (pbuf q = p; q != null; q = q.next) {
				int len = rest;
				if (len > q.len)
					len = q.len;

				Buffer.BlockCopy(q.payload.data, q.payload.offset, packet, pos, len);
				pos += len;
				rest -= len;
			}

			m_output(ip, packet, srch, desth, proto);

			return err_t.ERR_OK;
		}

		internal void input(pointer packet, int length, ip_addr srcn, ip_addr destn, byte proto)
		{
			ip_addr src = new ip_addr(lwip.lwip_htonl(srcn.addr));
			ip_addr dest = new ip_addr(lwip.lwip_htonl(destn.addr));
			pbuf p = pbuf_alloc(pbuf_layer.PBUF_RAW, (ushort)length, pbuf_type.PBUF_POOL);
			int pos = 0, rest = length;

			for (pbuf q = p; q != null; q = q.next) {
				int len = rest;
				if (len > q.len)
					len = q.len;

				pointer.memcpy(q.payload, new pointer(packet, pos), len);
				pos += len;
				rest -= len;
			}

			if (ip_input(p, src, dest, proto) != err_t.ERR_OK)
				pbuf_free(p);
		}
	}
}
