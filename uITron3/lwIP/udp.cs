/**
 * @file
 * User Datagram Protocol module
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


/* udp.c
 *
 * The code for the User Datagram Protocol UDP & UDPLite (RFC 3828).
 *
 */

/* @todo Check the use of '(udp_pcb).chksum_len_rx'!
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public partial class udp
	{
		lwip lwip;

		public udp(lwip lwip)
		{
			this.lwip = lwip;
		}

#if LWIP_UDP // don't build if not configured for use in lwipopts.h
		public const int UDP_HLEN = 8;
	}

	/* Fields are (of course) in network byte order. */
	public class udp_hdr : pointer
	{
		public new const int length = 8;
		pointer _src;
		pointer _dest;
		pointer _len;
		pointer _chksum;

		public udp_hdr(byte[] buffer, int offset)
			: base(buffer, offset)
		{
			_src = new pointer(data, offset + 0); /* ushort */
			_dest = new pointer(data, offset + 2); /* ushort */
			_len = new pointer(data, offset + 4); /* ushort */
			_chksum = new pointer(data, offset + 6); /* ushort */
		}

		public udp_hdr(byte[] buffer)
			: this(buffer, 0)
		{
		}

		public udp_hdr(pointer buffer)
			: this(buffer.data, buffer.offset)
		{
		}

		/* src/dest UDP ports */
		public ushort src { get { return (ushort)_src; } set { _src.SetValue(value); } }
		public ushort dest { get { return (ushort)_dest; } set { _dest.SetValue(value); } }
		public ushort len { get { return (ushort)_len; } set { _len.SetValue(value); } }
		public ushort chksum { get { return (ushort)_chksum; } set { _chksum.SetValue(value); } }
	};

	public partial class udp
	{
		public const byte UDP_FLAGS_NOCHKSUM = (byte)0x01U;
		public const byte UDP_FLAGS_UDPLITE = (byte)0x02U;
		public const byte UDP_FLAGS_CONNECTED = (byte)0x04U;
		public const byte UDP_FLAGS_MULTICAST_LOOP = (byte)0x08U;
	}

	/** Function prototype for udp pcb receive callback functions
	 * addr and port are in same byte order as in the pcb
	 * The callback is responsible for freeing the pbuf
	 * if it's not used any more.
	 *
	 * ATTENTION: Be aware that 'addr' points into the pbuf 'p' so freeing this pbuf
	 *            makes 'addr' invalid, too.
	 *
	 * @param arg user supplied argument (udp_pcb.recv_arg)
	 * @param pcb the udp_pcb which received data
	 * @param p the packet buffer that was received
	 * @param addr the remote IP address from which the packet was received
	 * @param port the remote port from which the packet was received
	 */
	public delegate void udp_recv_fn(object arg, udp_pcb pcb, pbuf p,
		ip_addr addr, ushort port);


	public class udp_pcb : ip_pcb
	{
		public udp_pcb(lwip lwip)
			: base(lwip)
		{
		}

		/* Protocol specific PCB members */
		public udp_pcb next;

		public byte flags;
		/** ports are in host byte order */
		public ushort local_port, remote_port;

#if LWIP_IGMP
		/** outgoing network interface for multicast packets */
		public ip_addr multicast_ip;
#endif // LWIP_IGMP

#if LWIP_UDPLITE
		/** used for UDP_LITE only */
		public ushort chksum_len_rx, chksum_len_tx;
#endif // LWIP_UDPLITE

		/** receive callback function */
		public udp_recv_fn recv;
		/** user-supplied argument for the recv callback */
		public object recv_arg;
	};

	public partial class udp
	{
		public static byte udp_flags(udp_pcb pcb) { return pcb.flags; }
		public static void udp_setflags(udp_pcb pcb, byte f) { pcb.flags = f; }

#if !UDP_DEBUG
		static void udp_debug_print(udp_hdr udphdr) { }
#endif

		/* From http://www.iana.org/assignments/port-numbers:
		   "The Dynamic and/or Private Ports are those from 49152 through 65535" */
		public const int UDP_LOCAL_PORT_RANGE_START = 0xc000;
		public const int UDP_LOCAL_PORT_RANGE_END = 0xffff;
		public static ushort UDP_ENSURE_LOCAL_PORT_RANGE(ushort port) { return (ushort)((port & ~UDP_LOCAL_PORT_RANGE_START) + UDP_LOCAL_PORT_RANGE_START); }

		/* last local UDP port */
		ushort udp_port = UDP_LOCAL_PORT_RANGE_START;

		/* The list of UDP PCBs */
		/* exported in udp.h (was static) */
		private udp_pcb udp_pcbs;

		/**
		 * Initialize this module.
		 */
		public static void udp_init(lwip lwip)
		{
			lwip.udp = new udp(lwip);
#if LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS && LWIP_RAND
			lwip.udp.udp_port = UDP_ENSURE_LOCAL_PORT_RANGE((ushort)sys.LWIP_RAND());
#endif // LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS && LWIP_RAND
		}

		/**
		 * Allocate a new local UDP port.
		 *
		 * @return a new (free) local UDP port number
		 */
		private ushort udp_new_port()
		{
			ushort n = 0;
			udp_pcb pcb;

		again:
			if (udp_port++ == UDP_LOCAL_PORT_RANGE_END)
			{
				udp_port = UDP_LOCAL_PORT_RANGE_START;
			}
			/* Check all PCBs. */
			for (pcb = udp_pcbs; pcb != null; pcb = pcb.next)
			{
				if (pcb.local_port == udp_port)
				{
					if (++n > (UDP_LOCAL_PORT_RANGE_END - UDP_LOCAL_PORT_RANGE_START))
					{
						return 0;
					}
					goto again;
				}
			}
			return udp_port;
#if false
			udp_pcb ipcb = udp_pcbs;
			while ((ipcb != null) && (udp_port != UDP_LOCAL_PORT_RANGE_END))
			{
				if (ipcb.local_port == udp_port)
				{
					/* port is already used by another udp_pcb */
					udp_port++;
					/* restart scanning all udp pcbs */
					ipcb = udp_pcbs;
				}
				else
				{
					/* go on with next udp pcb */
					ipcb = ipcb.next;
				}
			}
			if (ipcb != null)
			{
				return 0;
			}
			return udp_port;
#endif
		}

		/**
		 * Process an incoming UDP datagram.
		 *
		 * Given an incoming UDP datagram (as a chain of pbufs) this function
		 * finds a corresponding UDP PCB and hands over the pbuf to the pcbs
		 * recv function. If no pcb is found or the datagram is incorrect, the
		 * pbuf is freed.
		 *
		 * @param p pbuf to be demultiplexed to a UDP PCB.
		 * @param inp network interface on which the datagram was received.
		 *
		 */
		public void udp_input(pbuf p, lwip inp)
		{
			udp_hdr udphdr;
			udp_pcb pcb, prev;
			udp_pcb uncon_pcb;
			ip_hdr iphdr;
			ushort src, dest;
			byte local_match;
			bool broadcast;

			//PERF_START;

			++lwip.lwip_stats.udp.recv;

			iphdr = new ip_hdr(p.payload);

			/* Check minimum length (IP header + UDP header)
			 * and move payload pointer to UDP header */
			if (p.tot_len < (ip_hdr.IPH_HL(iphdr) * 4 + UDP_HLEN) || lwip.pbuf_header(p, (short)-(ip_hdr.IPH_HL(iphdr) * 4)) != 0)
			{
				/* drop short packets */
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG,
							"udp_input: short UDP datagram ({0} bytes) discarded\n", p.tot_len);
				++lwip.lwip_stats.udp.lenerr;
				++lwip.lwip_stats.udp.drop;
				//snmp.snmp_inc_udpinerrors();
				lwip.pbuf_free(p);
				goto end;
			}

			udphdr = new udp_hdr(p.payload);

			/* is broadcast packet ? */
			broadcast = ip_addr.ip_addr_isbroadcast(lwip.current_iphdr_dest, inp);

			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_input: received datagram of length {0}\n", p.tot_len);

			/* convert src and dest ports to host byte order */
			src = lwip.lwip_ntohs(udphdr.src);
			dest = lwip.lwip_ntohs(udphdr.dest);

			udp_debug_print(udphdr);

			/* print the UDP source and destination */
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG,
						"udp ({0}.{1}.{2}.{3}, {4}) <-- "
						 + "({5}.{6}.{7}.{8}, {9})\n",
						 ip_addr.ip4_addr1_16(iphdr.dest), ip_addr.ip4_addr2_16(iphdr.dest),
						 ip_addr.ip4_addr3_16(iphdr.dest), ip_addr.ip4_addr4_16(iphdr.dest), lwip.lwip_ntohs(udphdr.dest),
						 ip_addr.ip4_addr1_16(iphdr.src), ip_addr.ip4_addr2_16(iphdr.src),
						 ip_addr.ip4_addr3_16(iphdr.src), ip_addr.ip4_addr4_16(iphdr.src), lwip.lwip_ntohs(udphdr.src));

#if LWIP_DHCP
			pcb = null;
			/* when LWIP_DHCP is active, packets to DHCP_CLIENT_PORT may only be processed by
			   the dhcp module, no other UDP pcb may use the local UDP port DHCP_CLIENT_PORT */
			if (dest == dhcp.DHCP_CLIENT_PORT)
			{
				/* all packets for DHCP_CLIENT_PORT not coming from DHCP_SERVER_PORT are dropped! */
				if (src == dhcp.DHCP_SERVER_PORT)
				{
					if ((inp.dhcp != null) && (inp.dhcp.pcb != null))
					{
						/* accept the packe if 
						   (- broadcast or directed to us) . DHCP is link-layer-addressed, local ip is always ANY!
						   - inp.dhcp.pcb.remote == ANY or iphdr.src */
						if ((ip_addr.ip_addr_isany(inp.dhcp.pcb.remote_ip) ||
							ip_addr.ip_addr_cmp(inp.dhcp.pcb.remote_ip, lwip.current_iphdr_src)))
						{
							pcb = inp.dhcp.pcb;
						}
					}
				}
			}
			else
#endif // LWIP_DHCP
			{
				prev = null;
				local_match = 0;
				uncon_pcb = null;
				/* Iterate through the UDP pcb list for a matching pcb.
				 * 'Perfect match' pcbs (connected to the remote port & ip address) are
				 * preferred. If no perfect match is found, the first unconnected pcb that
				 * matches the local port and ip address gets the datagram. */
				for (pcb = udp_pcbs; pcb != null; pcb = pcb.next)
				{
					local_match = 0;
					/* print the PCB local and remote address */
					lwip.LWIP_DEBUGF(opt.UDP_DEBUG,
								"pcb ({0}.{1}.{2}.{3}, {4}) --- "
								 + "({5}.{6}.{7}.{8}, {9})\n",
								 ip_addr.ip4_addr1_16(pcb.local_ip), ip_addr.ip4_addr2_16(pcb.local_ip),
								 ip_addr.ip4_addr3_16(pcb.local_ip), ip_addr.ip4_addr4_16(pcb.local_ip), pcb.local_port,
								 ip_addr.ip4_addr1_16(pcb.remote_ip), ip_addr.ip4_addr2_16(pcb.remote_ip),
								 ip_addr.ip4_addr3_16(pcb.remote_ip), ip_addr.ip4_addr4_16(pcb.remote_ip), pcb.remote_port);

					/* compare PCB local addr+port to UDP destination addr+port */
					if (pcb.local_port == dest)
					{
						if ((!broadcast && ip_addr.ip_addr_isany(pcb.local_ip)) ||
							ip_addr.ip_addr_cmp(pcb.local_ip, lwip.current_iphdr_dest) ||
#if LWIP_IGMP
							ip_addr.ip_addr_ismulticast(lwip.current_iphdr_dest) ||
#endif // LWIP_IGMP
#if IP_SOF_BROADCAST_RECV
							(broadcast && lwip.ip_get_option(pcb, sof.SOF_BROADCAST) &&
								(ip_addr.ip_addr_isany(pcb.local_ip) ||
									ip_addr.ip_addr_netcmp(pcb.local_ip, lwip.ip_current_dest_addr(), inp.netmask))))
#else // IP_SOF_BROADCAST_RECV
							(broadcast &&
								(ip_addr.ip_addr_isany(pcb.local_ip) ||
								 ip_addr.ip_addr_netcmp(pcb.local_ip, lwip.ip_current_dest_addr(), inp.netmask))))
#endif // IP_SOF_BROADCAST_RECV
						{
							local_match = 1;
							if ((uncon_pcb == null) &&
								((pcb.flags & UDP_FLAGS_CONNECTED) == 0))
							{
								/* the first unconnected matching PCB */
								uncon_pcb = pcb;
							}
						}
					}
					/* compare PCB remote addr+port to UDP source addr+port */
					if ((local_match != 0) &&
						(pcb.remote_port == src) &&
						(ip_addr.ip_addr_isany(pcb.remote_ip) ||
						 ip_addr.ip_addr_cmp(pcb.remote_ip, lwip.current_iphdr_src)))
					{
						/* the first fully matching PCB */
						if (prev != null)
						{
							/* move the pcb to the front of udp_pcbs so that is
							   found faster next time */
							prev.next = pcb.next;
							pcb.next = udp_pcbs;
							udp_pcbs = pcb;
						}
						else
						{
							++lwip.lwip_stats.udp.cachehit;
						}
						break;
					}
					prev = pcb;
				}
				/* no fully matching pcb found? then look for an unconnected pcb */
				if (pcb == null)
				{
					pcb = uncon_pcb;
				}
			}

			/* Check checksum if this is a match or if it was directed at us. */
			if (pcb != null || ip_addr.ip_addr_cmp(inp.ip_addr, lwip.current_iphdr_dest))
			{
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, "udp_input: calculating checksum\n");
#if LWIP_UDPLITE
				if (ip_hdr.IPH_PROTO(iphdr) == lwip.IP_PROTO_UDPLITE)
				{
					/* Do the UDP Lite checksum */
#if CHECKSUM_CHECK_UDP
					ushort chklen = lwip.lwip_ntohs(udphdr.len);
					if (chklen < udp_hdr.length)
					{
						if (chklen == 0)
						{
							/* For UDP-Lite, checksum length of 0 means checksum
								over the complete packet (See RFC 3828 chap. 3.1) */
							chklen = p.tot_len;
						}
						else
						{
							/* At least the UDP-Lite header must be covered by the
								checksum! (Again, see RFC 3828 chap. 3.1) */
							++lwip.lwip_stats.udp.chkerr;
							++lwip.lwip_stats.udp.drop;
							//snmp.snmp_inc_udpinerrors();
							lwip.pbuf_free(p);
							goto end;
						}
					}
					if (lwip.inet_chksum_pseudo_partial(p, lwip.current_iphdr_src, lwip.current_iphdr_dest,
											 lwip.IP_PROTO_UDPLITE, p.tot_len, chklen) != 0)
					{
						lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
								("udp_input: UDP Lite datagram discarded due to failing checksum\n"));
						++lwip.lwip_stats.udp.chkerr;
						++lwip.lwip_stats.udp.drop;
						//snmp.snmp_inc_udpinerrors();
						lwip.pbuf_free(p);
						goto end;
					}
#endif // CHECKSUM_CHECK_UDP
				}
				else
#endif // LWIP_UDPLITE
				{
#if CHECKSUM_CHECK_UDP
					if (udphdr.chksum != 0)
					{
						if (lwip.inet_chksum_pseudo(p, lwip.ip_current_src_addr(), lwip.ip_current_dest_addr(),
								lwip.IP_PROTO_UDP, p.tot_len) != 0)
						{
							lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
										("udp_input: UDP datagram discarded due to failing checksum\n"));
							++lwip.lwip_stats.udp.chkerr;
							++lwip.lwip_stats.udp.drop;
							//snmp.snmp_inc_udpinerrors();
							lwip.pbuf_free(p);
							goto end;
						}
					}
#endif // CHECKSUM_CHECK_UDP
				}
				if (lwip.pbuf_header(p, -UDP_HLEN) != 0)
				{
					/* Can we cope with this failing? Just assert for now */
					lwip.LWIP_ASSERT("pbuf_header failed\n", false);
					++lwip.lwip_stats.udp.drop;
					//snmp.snmp_inc_udpinerrors();
					lwip.pbuf_free(p);
					goto end;
				}
				if (pcb != null)
				{
					//snmp.snmp_inc_udpindatagrams();
#if SO_REUSE && SO_REUSE_RXTOALL
					if ((broadcast || ip_addr.ip_addr_ismulticast(lwip.current_iphdr_dest)) &&
						lwip.ip_get_option(pcb, sof.SOF_REUSEADDR))
					{
						/* pass broadcast- or multicast packets to all multicast pcbs
						   if sof.SOF_REUSEADDR is set on the first match */
						udp_pcb mpcb;
						byte p_header_changed = 0;
						for (mpcb = udp_pcbs; mpcb != null; mpcb = mpcb.next)
						{
							if (mpcb != pcb)
							{
								/* compare PCB local addr+port to UDP destination addr+port */
								if ((mpcb.local_port == dest) &&
									((!broadcast && ip_addr.ip_addr_isany(mpcb.local_ip)) ||
										ip_addr.ip_addr_cmp(mpcb.local_ip, lwip.current_iphdr_dest) ||
#if LWIP_IGMP
										ip_addr.ip_addr_ismulticast(lwip.current_iphdr_dest) ||
#endif // LWIP_IGMP
#if IP_SOF_BROADCAST_RECV
										(broadcast && lwip.ip_get_option(mpcb, sof.SOF_BROADCAST))))
								{
#else  // IP_SOF_BROADCAST_RECV
										(broadcast)))
								{
#endif // IP_SOF_BROADCAST_RECV
									/* pass a copy of the packet to all local matches */
									if (mpcb.recv != null)
									{
										pbuf q;
										/* for that, move payload to IP header again */
										if (p_header_changed == 0)
										{
											lwip.pbuf_header(p, (short)((ip_hdr.IPH_HL(iphdr) * 4) + UDP_HLEN));
											p_header_changed = 1;
										}
										q = lwip.pbuf_alloc(pbuf_layer.PBUF_RAW, p.tot_len, pbuf_type.PBUF_RAM);
										if (q != null)
										{
											err_t err = lwip.pbuf_copy(q, p);
											if (err == err_t.ERR_OK)
											{
												/* move payload to UDP data */
												lwip.pbuf_header(q, (short)-((ip_hdr.IPH_HL(iphdr) * 4) + UDP_HLEN));
												mpcb.recv(mpcb.recv_arg, mpcb, q, lwip.ip_current_src_addr(), src);
											}
										}
									}
								}
							}
						}
						if (p_header_changed != 0)
						{
							/* and move payload to UDP data again */
							lwip.pbuf_header(p, (short)-((ip_hdr.IPH_HL(iphdr) * 4) + UDP_HLEN));
						}
					}
#endif // SO_REUSE && SO_REUSE_RXTOALL
					/* callback */
					if (pcb.recv != null)
					{
						/* now the recv function is responsible for freeing p */
						pcb.recv(pcb.recv_arg, pcb, p, lwip.ip_current_src_addr(), src);
					}
					else
					{
						/* no recv function registered? then we have to free the pbuf! */
						lwip.pbuf_free(p);
						goto end;
					}
				}
				else
				{
					lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, "udp_input: not for us.\n");

#if LWIP_ICMP
					/* No match was found, send ICMP destination port unreachable unless
					   destination address was broadcast/multicast. */
					if (!broadcast &&
						!ip_addr.ip_addr_ismulticast(lwip.current_iphdr_dest))
					{
						/* move payload pointer back to ip header */
						lwip.pbuf_header(p, (short)((ip_hdr.IPH_HL(iphdr) * 4) + UDP_HLEN));
						lwip.LWIP_ASSERT("p.payload == iphdr", (p.payload == iphdr));
						lwip.icmp.icmp_dest_unreach(p, icmp_dur_type.ICMP_DUR_PORT);
					}
#endif // LWIP_ICMP
					++lwip.lwip_stats.udp.proterr;
					++lwip.lwip_stats.udp.drop;
					//snmp.snmp_inc_udpnoports();
					lwip.pbuf_free(p);
				}
			}
			else
			{
				lwip.pbuf_free(p);
			}
		end:
			;//PERF_STOP("udp_input");
		}

		/**
		 * Send data using UDP.
		 *
		 * @param pcb UDP PCB used to send the data.
		 * @param p chain of pbuf's to be sent.
		 *
		 * The datagram will be sent to the current remote_ip & remote_port
		 * stored in pcb. If the pcb is not bound to a port, it will
		 * automatically be bound to a random port.
		 *
		 * @return lwIP error code.
		 * - err_t.ERR_OK. Successful. No error occured.
		 * - err_t.ERR_MEM. Out of memory.
		 * - err_t.ERR_RTE. Could not find route to destination address.
		 * - More errors could be returned by lower protocol layers.
		 *
		 * @see udp_disconnect() udp_sendto()
		 */
		public err_t udp_send(udp_pcb pcb, pbuf p)
		{
			/* send to the packet using remote ip and port stored in the pcb */
			return udp_sendto(pcb, p, pcb.remote_ip, pcb.remote_port);
		}

#if LWIP_CHECKSUM_ON_COPY
		/** Same as udp_send() but with checksum
		 */
		public err_t udp_send_chksum(udp_pcb pcb, pbuf p,
						byte have_chksum, ushort chksum)
		{
			/* send to the packet using remote ip and port stored in the pcb */
			return udp_sendto_chksum(pcb, p, pcb.remote_ip, pcb.remote_port,
			  have_chksum, chksum);
		}
#endif // LWIP_CHECKSUM_ON_COPY

		/**
		 * Send data to a specified address using UDP.
		 *
		 * @param pcb UDP PCB used to send the data.
		 * @param p chain of pbuf's to be sent.
		 * @param dst_ip Destination IP address.
		 * @param dst_port Destination UDP port.
		 *
		 * dst_ip & dst_port are expected to be in the same byte order as in the pcb.
		 *
		 * If the PCB already has a remote address association, it will
		 * be restored after the data is sent.
		 * 
		 * @return lwIP error code (@see udp_send for possible error codes)
		 *
		 * @see udp_disconnect() udp_send()
		 */
		public err_t udp_sendto(udp_pcb pcb, pbuf p,
			ip_addr dst_ip, ushort dst_port)
		{
#if LWIP_CHECKSUM_ON_COPY
			return udp_sendto_chksum(pcb, p, dst_ip, dst_port, 0, 0);
		}

		/** Same as udp_sendto(), but with checksum */
		public err_t udp_sendto_chksum(udp_pcb pcb, pbuf p, ip_addr dst_ip,
			ushort dst_port, byte have_chksum, ushort chksum)
		{
#endif // LWIP_CHECKSUM_ON_COPY
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, "udp_send\n");
#if LWIP_CHECKSUM_ON_COPY
			return udp_sendto_if_chksum(pcb, p, dst_ip, dst_port, lwip, have_chksum, chksum);
#else // LWIP_CHECKSUM_ON_COPY
			return udp_sendto_if(pcb, p, dst_ip, dst_port, netif);
#endif // LWIP_CHECKSUM_ON_COPY
		}

		/**
		 * Send data to a specified address using UDP.
		 * The netif used for sending can be specified.
		 *
		 * This function exists mainly for DHCP, to be able to send UDP packets
		 * on a netif that is still down.
		 *
		 * @param pcb UDP PCB used to send the data.
		 * @param p chain of pbuf's to be sent.
		 * @param dst_ip Destination IP address.
		 * @param dst_port Destination UDP port.
		 * @param netif the netif used for sending.
		 *
		 * dst_ip & dst_port are expected to be in the same byte order as in the pcb.
		 *
		 * @return lwIP error code (@see udp_send for possible error codes)
		 *
		 * @see udp_disconnect() udp_send()
		 */
		public err_t udp_sendto_if(udp_pcb pcb, pbuf p, ip_addr dst_ip, ushort dst_port, lwip netif)
		{
#if LWIP_CHECKSUM_ON_COPY
			return udp_sendto_if_chksum(pcb, p, dst_ip, dst_port, netif, 0, 0);
		}

		/** Same as udp_sendto_if(), but with checksum */
		public err_t udp_sendto_if_chksum(udp_pcb pcb, pbuf p, ip_addr dst_ip,
			ushort dst_port, lwip netif, byte have_chksum, ushort chksum)
		{
#endif // LWIP_CHECKSUM_ON_COPY
			udp_hdr udphdr;
			ip_addr src_ip = new ip_addr(0);
			err_t err;
			pbuf q; /* q will be sent down the stack */

#if IP_SOF_BROADCAST
			/* broadcast filter? */
			if (!lwip.ip_get_option(pcb, sof.SOF_BROADCAST) && ip_addr.ip_addr_isbroadcast(dst_ip, netif))
			{
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
				  "udp_sendto_if: sof.SOF_BROADCAST not enabled on pcb {0}\n", pcb);
				return err_t.ERR_VAL;
			}
#endif // IP_SOF_BROADCAST

			/* if the PCB is not yet bound to a port, bind it here */
			if (pcb.local_port == 0)
			{
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, "udp_send: not yet bound to a port, binding now\n");
				err = udp_bind(pcb, pcb.local_ip, pcb.local_port);
				if (err != err_t.ERR_OK)
				{
					lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_LEVEL_SERIOUS, "udp_send: forced port bind failed\n");
					return err;
				}
			}

			/* not enough space to add an UDP header to first pbuf in given p chain? */
			if (lwip.pbuf_header(p, UDP_HLEN) != 0)
			{
				/* allocate header in a separate new pbuf */
				q = lwip.pbuf_alloc(pbuf_layer.PBUF_IP, UDP_HLEN, pbuf_type.PBUF_RAM);
				/* new header pbuf could not be allocated? */
				if (q == null)
				{
					lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_LEVEL_SERIOUS, "udp_send: could not allocate header\n");
					return err_t.ERR_MEM;
				}
				if (p.tot_len != 0)
				{
					/* chain header q in front of given pbuf p (only if p contains data) */
					lwip.pbuf_chain(q, p);
				}
				/* first pbuf q points to header pbuf */
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG,
							"udp_send: added header pbuf {0} before given pbuf {1}\n", q, p);
			}
			else
			{
				/* adding space for header within p succeeded */
				/* first pbuf q equals given pbuf */
				q = p;
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: added header in given pbuf {0}\n", p);
			}
			lwip.LWIP_ASSERT("check that first pbuf can hold udp_hdr",
						(q.len >= udp_hdr.length));
			/* q now represents the packet to be sent */
			udphdr = new udp_hdr(q.payload);
			udphdr.src = lwip.lwip_htons(pcb.local_port);
			udphdr.dest = lwip.lwip_htons(dst_port);
			/* in UDP, 0 checksum means 'no checksum' */
			udphdr.chksum = 0x0000;

			/* Multicast Loop? */
#if LWIP_IGMP
			if (ip_addr.ip_addr_ismulticast(dst_ip) && ((pcb.flags & UDP_FLAGS_MULTICAST_LOOP) != 0))
			{
				q.flags |= pbuf.PBUF_FLAG_MCASTLOOP;
			}
#endif // LWIP_IGMP


			/* PCB local address is IP_ANY_ADDR? */
			if (ip_addr.ip_addr_isany(pcb.local_ip))
			{
				/* use outgoing network interface IP address as source address */
				ip_addr.ip_addr_copy(src_ip, netif.ip_addr);
			}
			else
			{
				/* check if UDP PCB local IP address is correct
				 * this could be an old address if netif.ip_addr has changed */
				if (!ip_addr.ip_addr_cmp(pcb.local_ip, netif.ip_addr))
				{
					/* local_ip doesn't match, drop the packet */
					if (q != p)
					{
						/* free the header pbuf */
						lwip.pbuf_free(q);
						q = null;
						/* p is still referenced by the caller, and will live on */
					}
					return err_t.ERR_VAL;
				}
				/* use UDP PCB local IP address as source address */
				ip_addr.ip_addr_copy(src_ip, pcb.local_ip);
			}

			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: sending datagram of length {0}\n", q.tot_len);

#if LWIP_UDPLITE
			/* UDP Lite protocol? */
			if ((pcb.flags & UDP_FLAGS_UDPLITE) != 0)
			{
				ushort chklen, chklen_hdr;
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: UDP LITE packet length {0}\n", q.tot_len);
				/* set UDP message length in UDP header */
				chklen_hdr = chklen = pcb.chksum_len_tx;
				if ((chklen < udp_hdr.length) || (chklen > q.tot_len))
				{
					if (chklen != 0)
					{
						lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: UDP LITE pcb.chksum_len is illegal: {0}\n", chklen);
					}
					/* For UDP-Lite, checksum length of 0 means checksum
						over the complete packet. (See RFC 3828 chap. 3.1)
						At least the UDP-Lite header must be covered by the
						checksum, therefore, if chksum_len has an illegal
						value, we generate the checksum over the complete
						packet to be safe. */
					chklen_hdr = 0;
					chklen = q.tot_len;
				}
				udphdr.len = lwip.lwip_htons(chklen_hdr);
				/* calculate checksum */
#if CHECKSUM_GEN_UDP
				udphdr.chksum = lwip.inet_chksum_pseudo_partial(q, src_ip, dst_ip,
					lwip.IP_PROTO_UDPLITE, q.tot_len,
#if !LWIP_CHECKSUM_ON_COPY
					chklen);
#else // !LWIP_CHECKSUM_ON_COPY
					((have_chksum != 0) ? (ushort)UDP_HLEN : chklen));
				if (have_chksum != 0)
				{
					uint acc;
					acc = (uint)(udphdr.chksum + (ushort)~(chksum));
					udphdr.chksum = (ushort)lwip.FOLD_U32T(acc);
				}
#endif // !LWIP_CHECKSUM_ON_COPY

				/* chksum zero must become 0xffff, as zero means 'no checksum' */
				if (udphdr.chksum == 0x0000)
				{
					udphdr.chksum = 0xffff;
				}
#endif // CHECKSUM_GEN_UDP
				/* output to IP */
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: ip_output_if (,,,,lwip.IP_PROTO_UDPLITE,)\n");
				err = lwip.ip_output_if(q, src_ip, dst_ip, pcb.ttl, pcb.tos, lwip.IP_PROTO_UDPLITE);
			}
			else
#endif // LWIP_UDPLITE
			{      /* UDP */
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: UDP packet length {0}\n", q.tot_len);
				udphdr.len = lwip.lwip_htons(q.tot_len);
				/* calculate checksum */
#if CHECKSUM_GEN_UDP
				if ((pcb.flags & udp.UDP_FLAGS_NOCHKSUM) == 0)
				{
					ushort udpchksum;
#if LWIP_CHECKSUM_ON_COPY
					if (have_chksum != 0)
					{
						uint acc;
						udpchksum = lwip.inet_chksum_pseudo_partial(q, src_ip, dst_ip, lwip.IP_PROTO_UDP,
						  q.tot_len, UDP_HLEN);
						acc = (uint)(udpchksum + (ushort)~(chksum));
						udpchksum = (ushort)lwip.FOLD_U32T(acc);
					}
					else
#endif // LWIP_CHECKSUM_ON_COPY
					{
						udpchksum = lwip.inet_chksum_pseudo(q, src_ip, dst_ip, lwip.IP_PROTO_UDP, q.tot_len);
					}

					/* chksum zero must become 0xffff, as zero means 'no checksum' */
					if (udpchksum == 0x0000)
					{
						udpchksum = 0xffff;
					}
					udphdr.chksum = udpchksum;
				}
#endif // CHECKSUM_GEN_UDP
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: UDP checksum 0x{0:X}\n", udphdr.chksum);
				lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_send: ip_output_if (,,,,lwip.IP_PROTO_UDP,)\n");
				/* output to IP */
				err = lwip.ip_output_if(q, src_ip, dst_ip, pcb.ttl, pcb.tos, lwip.IP_PROTO_UDP);
			}
			/* TODO: must this be increased even if error occured? */
			//snmp.snmp_inc_udpoutdatagrams();

			/* did we chain a separate header pbuf earlier? */
			if (q != p)
			{
				/* free the header pbuf */
				lwip.pbuf_free(q);
				q = null;
				/* p is still referenced by the caller, and will live on */
			}

			++lwip.lwip_stats.udp.xmit;
			return err;
		}

		/**
		 * Bind an UDP PCB.
		 *
		 * @param pcb UDP PCB to be bound with a local address ipaddr and port.
		 * @param ipaddr local IP address to bind with. Use IP_ADDR_ANY to
		 * bind to all local interfaces.
		 * @param port local UDP port to bind with. Use 0 to automatically bind
		 * to a random port between UDP_LOCAL_PORT_RANGE_START and
		 * UDP_LOCAL_PORT_RANGE_END.
		 *
		 * ipaddr & port are expected to be in the same byte order as in the pcb.
		 *
		 * @return lwIP error code.
		 * - err_t.ERR_OK. Successful. No error occured.
		 * - err_t.ERR_USE. The specified ipaddr and port are already bound to by
		 * another UDP PCB.
		 *
		 * @see udp_disconnect()
		 */
		public err_t udp_bind(udp_pcb pcb, ip_addr ipaddr, ushort port)
		{
			udp_pcb ipcb;
			byte rebind;

			lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, "udp_bind(ipaddr = ");
			ip_addr.ip_addr_debug_print(opt.UDP_DEBUG, ipaddr);
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE, ", port = {0})\n", port);

			rebind = 0;
			/* Check for double bind and rebind of the same pcb */
			for (ipcb = udp_pcbs; ipcb != null; ipcb = ipcb.next)
			{
				/* is this UDP PCB already on active list? */
				if (pcb == ipcb)
				{
					/* pcb may occur at most once in active list */
					lwip.LWIP_ASSERT("rebind == 0", rebind == 0);
					/* pcb already in list, just rebind */
					rebind = 1;
				}

				/* By default, we don't allow to bind to a port that any other udp
				   PCB is alread bound to, unless all PCBs with that port have tha
				   REUSEADDR flag set. */
#if SO_REUSE
				else if (!lwip.ip_get_option(pcb, (byte)sof.SOF_REUSEADDR) &&
						 !lwip.ip_get_option(ipcb, (byte)sof.SOF_REUSEADDR))
				{
#else // SO_REUSE
				/* port matches that of PCB in list and REUSEADDR not set . reject */
				else
				{
#endif // SO_REUSE
					if ((ipcb.local_port == port) &&
						/* IP address matches, or one is IP_ADDR_ANY? */
						(ip_addr.ip_addr_isany(ipcb.local_ip) ||
						 ip_addr.ip_addr_isany(ipaddr) ||
						 ip_addr.ip_addr_cmp(ipcb.local_ip, ipaddr)))
					{
						/* other PCB already binds to this local IP and port */
						lwip.LWIP_DEBUGF(opt.UDP_DEBUG,
									"udp_bind: local port {0} already bound by another pcb\n", port);
						return err_t.ERR_USE;
					}
				}
			}

			ip_addr.ip_addr_set(pcb.local_ip, ipaddr);

			/* no port specified? */
			if (port == 0)
			{
				port = udp_new_port();
				if (port == 0)
				{
					/* no more ports available in local range */
					lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "udp_bind: out of free UDP ports\n");
					return err_t.ERR_USE;
				}
			}
			pcb.local_port = port;
			//snmp.snmp_insert_udpidx_tree(pcb);
			/* pcb not active yet? */
			if (rebind == 0)
			{
				/* place the PCB on the active list if not already there */
				pcb.next = udp_pcbs;
				udp_pcbs = pcb;
			}
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_STATE,
						"udp_bind: bound to {0}.{1}.{2}.{3}, port {4}\n",
						ip_addr.ip4_addr1_16(pcb.local_ip), ip_addr.ip4_addr2_16(pcb.local_ip),
						ip_addr.ip4_addr3_16(pcb.local_ip), ip_addr.ip4_addr4_16(pcb.local_ip),
						pcb.local_port);
			return err_t.ERR_OK;
		}

		/**
		 * Connect an UDP PCB.
		 *
		 * This will associate the UDP PCB with the remote address.
		 *
		 * @param pcb UDP PCB to be connected with remote address ipaddr and port.
		 * @param ipaddr remote IP address to connect with.
		 * @param port remote UDP port to connect with.
		 *
		 * @return lwIP error code
		 *
		 * ipaddr & port are expected to be in the same byte order as in the pcb.
		 *
		 * The udp pcb is bound to a random local port if not already bound.
		 *
		 * @see udp_disconnect()
		 */
		public err_t udp_connect(udp_pcb pcb, ip_addr ipaddr, ushort port)
		{
			udp_pcb ipcb;

			if (pcb.local_port == 0)
			{
				err_t err = udp_bind(pcb, pcb.local_ip, pcb.local_port);
				if (err != err_t.ERR_OK)
				{
					return err;
				}
			}

			ip_addr.ip_addr_set(pcb.remote_ip, ipaddr);
			pcb.remote_port = port;
			pcb.flags |= UDP_FLAGS_CONNECTED;
			/** TODO: this functionality belongs in upper layers */
#if LWIP_UDP_TODO
			/* Nail down local IP for netconn_addr()/getsockname() */
			if (ip_addr.ip_addr_isany(pcb.local_ip) && !ip_addr.ip_addr_isany(pcb.remote_ip))
			{
				/** TODO: this will bind the udp pcb locally, to the interface which
					is used to route output packets to the remote address. However, we
					might want to accept incoming packets on any interface! */
				ip_addr.ip_addr_copy(pcb.local_ip, lwip.ip_addr);
			}
			else if (ip_addr.ip_addr_isany(pcb.remote_ip))
			{
				ip_addr.ip_addr_copy(pcb.local_ip, new ip_addr(0));
			}
#endif
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_STATE,
						"udp_connect: connected to {0}.{1}.{2}.{3},port {4}\n",
						 ip_addr.ip4_addr1_16(pcb.local_ip), ip_addr.ip4_addr2_16(pcb.local_ip),
						 ip_addr.ip4_addr3_16(pcb.local_ip), ip_addr.ip4_addr4_16(pcb.local_ip),
						 pcb.local_port);

			/* Insert UDP PCB into the list of active UDP PCBs. */
			for (ipcb = udp_pcbs; ipcb != null; ipcb = ipcb.next)
			{
				if (pcb == ipcb)
				{
					/* already on the list, just return */
					return err_t.ERR_OK;
				}
			}
			/* PCB not yet on the list, add PCB now */
			pcb.next = udp_pcbs;
			udp_pcbs = pcb;
			return err_t.ERR_OK;
		}

		/**
		 * Disconnect a UDP PCB
		 *
		 * @param pcb the udp pcb to disconnect.
		 */
		public static void udp_disconnect(udp_pcb pcb)
		{
			/* reset remote address association */
			ip_addr.ip_addr_set_any(pcb.remote_ip);
			pcb.remote_port = 0;
			/* mark PCB as unconnected */
			pcb.flags &= unchecked((byte)~UDP_FLAGS_CONNECTED);
		}

		/**
		 * Set a receive callback for a UDP PCB
		 *
		 * This callback will be called when receiving a datagram for the pcb.
		 *
		 * @param pcb the pcb for wich to set the recv callback
		 * @param recv function pointer of the callback function
		 * @param recv_arg additional argument to pass to the callback function
		 */
		public static void udp_recv(udp_pcb pcb, udp_recv_fn recv, object recv_arg)
		{
			/* remember recv() callback and user data */
			pcb.recv = recv;
			pcb.recv_arg = recv_arg;
		}

		/**
		 * Remove an UDP PCB.
		 *
		 * @param pcb UDP PCB to be removed. The PCB is removed from the list of
		 * UDP PCB's and the data structure is freed from memory.
		 *
		 * @see udp_new()
		 */
		public void udp_remove(udp_pcb pcb)
		{
			udp_pcb pcb2;

			//snmp.snmp_delete_udpidx_tree(pcb);
			/* pcb to be removed is first in list? */
			if (udp_pcbs == pcb)
			{
				/* make list start at 2nd pcb */
				udp_pcbs = udp_pcbs.next;
				/* pcb not 1st in list */
			}
			else
			{
				for (pcb2 = udp_pcbs; pcb2 != null; pcb2 = pcb2.next)
				{
					/* find pcb in udp_pcbs list */
					if (pcb2.next != null && pcb2.next == pcb)
					{
						/* remove pcb from list */
						pcb2.next = pcb.next;
					}
				}
			}
			lwip.memp_free(memp_t.MEMP_UDP_PCB, pcb);
		}

		/**
		 * Create a UDP PCB.
		 *
		 * @return The UDP PCB which was created. null if the PCB data structure
		 * could not be allocated.
		 *
		 * @see udp_remove()
		 */
		public udp_pcb udp_new()
		{
			udp_pcb pcb;
			pcb = (udp_pcb)lwip.memp_malloc(memp_t.MEMP_UDP_PCB);
			/* could allocate UDP PCB? */
			if (pcb != null)
			{
				/* UDP Lite: by initializing to all zeroes, chksum_len is set to 0
				 * which means checksum is generated over the whole datagram per default
				 * (recommended as default by RFC 3828). */
				/* initialize PCB to all zeroes */
				memp.memset(pcb, 0, udp_pcb.length);
				pcb.ttl = opt.UDP_TTL;
			}
			return pcb;
		}

#if UDP_DEBUG
		/**
		 * Print UDP header information for debug purposes.
		 *
		 * @param udphdr pointer to the udp header in memory.
		 */
		static void
		udp_debug_print(udp_hdr udphdr)
		{
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "UDP header:\n");
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "|     {0,5}     |     {1,5}     | (src port, dest port)\n",
									lwip.lwip_ntohs(udphdr.src), lwip.lwip_ntohs(udphdr.dest));
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "|     {0,5}     |     0x{1:X4}    | (len, chksum)\n",
									lwip.lwip_ntohs(udphdr.len), lwip.lwip_ntohs(udphdr.chksum));
			lwip.LWIP_DEBUGF(opt.UDP_DEBUG, "+-------------------------------+\n");
		}
#endif // UDP_DEBUG

#endif // LWIP_UDP
	}

	partial class lwip
	{
		internal udp udp;
	}
}
