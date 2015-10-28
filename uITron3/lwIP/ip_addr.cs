/**
 * @file
 * This is the IPv4 address tools implementation.
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	/* This is the aligned version of ip_addr,
	   used as local variable, on the stack, etc. */
	public partial class ip_addr : pointer
	{
		public new const int length = 4;

		public ip_addr(byte[] buffer, int offset)
			: base(buffer, offset)
		{
		}

		public ip_addr(byte[] buffer)
			: this(buffer, 0)
		{
		}

		public ip_addr(uint addr)
			: base(BitConverter.GetBytes(addr), 0)
		{
		}

		public ip_addr(pointer buffer)
			: base(buffer.data, buffer.offset)
		{
		}

		public uint addr
		{
			get { return BitConverter.ToUInt32(data, offset); }
			set { SetValue(value); }
		}

		/** IP_ADDR_ can be used as a fixed IP address
		 *  for the wildcard and the broadcast address
		 */
		public static readonly ip_addr IP_ADDR_ANY = ip_addr_any;
		public static readonly ip_addr IP_ADDR_BROADCAST = ip_addr_broadcast;

		/** 255.255.255.255 */
		public const uint IPADDR_NONE = ((uint)0xffffffffUL);
		/** 127.0.0.1 */
		public const uint IPADDR_LOOPBACK = ((uint)0x7f000001UL);
		/** 0.0.0.0 */
		public const uint IPADDR_ANY = ((uint)0x00000000UL);
		/** 255.255.255.255 */
		public const uint IPADDR_BROADCAST = ((uint)0xffffffffUL);

		/* Definitions of the bits in an Internet address integer.

		   On subnets, host and network parts are found according to
		   the subnet mask, not these masks.  */
		public static bool IP_CLASSA(uint a) { return ((a & 0x80000000UL) == 0); }
		public const uint IP_CLASSA_NET = 0xff000000;
		public const int IP_CLASSA_NSHIFT = 24;
		public const uint IP_CLASSA_HOST = (0xffffffff & ~IP_CLASSA_NET);
		public const int IP_CLASSA_MAX = 128;

		public static bool IP_CLASSB(uint a) { return ((a & 0xc0000000UL) == 0x80000000UL); }
		public const uint IP_CLASSB_NET = 0xffff0000;
		public const int IP_CLASSB_NSHIFT = 16;
		public const uint IP_CLASSB_HOST = (0xffffffff & ~IP_CLASSB_NET);
		public const int IP_CLASSB_MAX = 65536;

		public static bool IP_CLASSC(uint a) { return ((a & 0xe0000000UL) == 0xc0000000UL); }
		public const uint IP_CLASSC_NET = 0xffffff00;
		public const int IP_CLASSC_NSHIFT = 8;
		public const uint IP_CLASSC_HOST = (0xffffffff & ~IP_CLASSC_NET);

		public static bool IP_CLASSD(uint a) { return ((a & 0xf0000000UL) == 0xe0000000UL); }
		public const uint IP_CLASSD_NET = 0xf0000000;          /* These ones aren't really */
		public const int IP_CLASSD_NSHIFT = 28;                  /*   net and host fields, but */
		public const uint IP_CLASSD_HOST = 0x0fffffff;          /*   routing needn't know. */
		public static bool IP_MULTICAST(uint a) { return IP_CLASSD(a); }

		public static bool IP_EXPERIMENTAL(uint a) { return ((a & 0xf0000000UL) == 0xf0000000UL); }
		public static bool IP_BADCLASS(uint a) { return ((a & 0xf0000000UL) == 0xf0000000UL); }

		public const int IP_LOOPBACKNET = 127;                 /* official! */

		/** Set an IP address given by the four byte-parts.
			Little-endian version that prevents the use of lwip.lwip_htonl. */
		public static void IP4_ADDR(ip_addr ipaddr, byte a, byte b, byte c, byte d)
		{
			ipaddr.addr = ((uint)((d) & 0xff) << 24) |
						  ((uint)((c) & 0xff) << 16) |
						  ((uint)((b) & 0xff) << 8) |
						   (uint)((a) & 0xff);
		}

		/** MEMCPY-like copying of IP addresses where addresses are known to be
		 * 16-bit-aligned if the port is correctly configured (so a port could define
		 * this to copying 2 ushort's) - no null-pointer-checking needed. */
		public static void IPADDR2_COPY(ip_addr dest, ip_addr src) { dest.addr = src.addr; }

		/** Copy IP address - faster than ip_addr.ip_addr_set: no null check */
		public static void ip_addr_copy(ip_addr dest, ip_addr src) { dest.addr = src.addr; }
		/** Safely copy one IP address to another (src may be null) */
		public static void ip_addr_set(ip_addr dest, ip_addr src)
		{
			dest.addr = (src == null ? 0 : src.addr);
		}
		/** Set complete address to zero */
		public static void ip_addr_set_zero(ip_addr ipaddr) { ipaddr.addr = 0; }
		/** Set address to IPADDR_ANY (no need for lwip.lwip_htonl()) */
		public static void ip_addr_set_any(ip_addr ipaddr) { ipaddr.addr = IPADDR_ANY; }
		/** Set address to loopback address */
		public static void ip_addr_set_loopback(ip_addr ipaddr) { ipaddr.addr = lwip.PP_HTONL(IPADDR_LOOPBACK); }
		/** Safely copy one IP address to another and change byte order
		 * from host- to network-order. */
		public static void ip_addr_set_hton(ip_addr dest, ip_addr src)
		{
			dest.addr =
				(src == null ? 0U :
				lwip.lwip_htonl(src.addr));
		}

		/** IPv4 only: set the IP address given as an uint */
		public static void ip4_addr_set_u32(ip_addr dest_ipaddr, uint src_u32) { dest_ipaddr.addr = src_u32; }
		/** IPv4 only: get the IP address as an uint */
		public static uint ip4_addr_get_u32(ip_addr src_ipaddr) { return src_ipaddr.addr; }

		/** Get the network address by combining host address with netmask */
		public static void ip_addr_get_network(ip_addr target, ip_addr host, ip_addr netmask) { target.addr = host.addr & netmask.addr; }

		/**
		 * Determine if two address are on the same network.
		 *
		 * @arg addr1 IP address 1
		 * @arg addr2 IP address 2
		 * @arg mask network identifier mask
		 * @return !0 if the network identifiers of both address match
		 */
		public static bool ip_addr_netcmp(ip_addr addr1, ip_addr addr2, ip_addr mask)
		{
			return (((addr1).addr &
				(mask).addr) ==
				((addr2).addr &
				(mask).addr));
		}
		public static bool ip_addr_cmp(ip_addr addr1, ip_addr addr2) { return ((addr1).addr == (addr2).addr); }

		public static bool ip_addr_isany(ip_addr addr1) { return ((addr1) == null || (addr1).addr == IPADDR_ANY); }

		public static bool ip_addr_isbroadcast(ip_addr ipaddr, netif netif) { return ip_addr.ip4_addr_isbroadcast(ipaddr.addr, netif); }

		public static bool ip_addr_netmask_valid(ip_addr netmask) { return ip_addr.ip4_addr_netmask_valid((netmask).addr); }

		public static bool ip_addr_ismulticast(ip_addr addr1) { return (((addr1).addr & lwip.PP_HTONL(0xf0000000U)) == lwip.PP_HTONL(0xe0000000U)); }

		public static bool ip_addr_islinklocal(ip_addr addr1) { return (((addr1).addr & lwip.PP_HTONL(0xffff0000U)) == lwip.PP_HTONL(0xa9fe0000U)); }

		public static void ip_addr_debug_print(uint debug, ip_addr ipaddr)
		{
			uITron3.lwip.LWIP_DEBUGF(debug, "{0}.{1}.{2}.{3}",
								ipaddr != null ? ip_addr.ip4_addr1_16(ipaddr) : 0,
								ipaddr != null ? ip_addr.ip4_addr2_16(ipaddr) : 0,
								ipaddr != null ? ip_addr.ip4_addr3_16(ipaddr) : 0,
								ipaddr != null ? ip_addr.ip4_addr4_16(ipaddr) : 0);
		}

		/* Get one byte from the 4-byte address */
		public static byte ip4_addr1(ip_addr ipaddr) { return BitConverter.GetBytes(ipaddr.addr)[0]; }
		public static byte ip4_addr2(ip_addr ipaddr) { return BitConverter.GetBytes(ipaddr.addr)[1]; }
		public static byte ip4_addr3(ip_addr ipaddr) { return BitConverter.GetBytes(ipaddr.addr)[2]; }
		public static byte ip4_addr4(ip_addr ipaddr) { return BitConverter.GetBytes(ipaddr.addr)[3]; }
		/* These are cast to ushort, with the intent that they are often arguments
		 * to printf using the U16_F format from cc.h. */
		public static ushort ip4_addr1_16(ip_addr ipaddr) { return ((ushort)ip_addr.ip4_addr1(ipaddr)); }
		public static ushort ip4_addr2_16(ip_addr ipaddr) { return ((ushort)ip_addr.ip4_addr2(ipaddr)); }
		public static ushort ip4_addr3_16(ip_addr ipaddr) { return ((ushort)ip_addr.ip4_addr3(ipaddr)); }
		public static ushort ip4_addr4_16(ip_addr ipaddr) { return ((ushort)ip_addr.ip4_addr4(ipaddr)); }

		/** For backwards compatibility */
		public static string ip_ntoa(ip_addr ipaddr) { return ipaddr_ntoa(ipaddr); }

		/* used by IP_ADDR_ANY and ip_addr.IP_ADDR_BROADCAST in ip_addr.h */
		private static ip_addr ip_addr_any = new ip_addr(lwip.lwip_htonl(IPADDR_ANY));
		private static ip_addr ip_addr_broadcast = new ip_addr(lwip.lwip_htonl(IPADDR_BROADCAST));

		/**
		 * Determine if an address is a broadcast address on a network interface 
		 * 
		 * @param addr address to be checked
		 * @param netif the network interface against which the address is checked
		 * @return returns non-zero if the address is a broadcast address
		 */
		public static bool ip4_addr_isbroadcast(uint addr, netif netif)
		{
			ip_addr ipaddr = new ip_addr(0);
			ip_addr.ip4_addr_set_u32(ipaddr, addr);

			/* all ones (broadcast) or all zeroes (old skool broadcast) */
			if ((~addr == IPADDR_ANY) ||
				(addr == IPADDR_ANY))
			{
				return true;
				/* no broadcast support on this network interface? */
			}
			else if ((netif.flags & netif.NETIF_FLAG_BROADCAST) == 0)
			{
				/* the given address cannot be a broadcast address
				 * nor can we check against any broadcast addresses */
				return false;
				/* address matches network interface address exactly? => no broadcast */
			}
			else if (addr == ip_addr.ip4_addr_get_u32(netif.ip_addr))
			{
				return false;
				/*  on the same (sub) network... */
			}
			else if (ip_addr.ip_addr_netcmp(ipaddr, netif.ip_addr, netif.netmask)
				  /* ...and host identifier bits are all ones? =>... */
				  && ((addr & ~ip_addr.ip4_addr_get_u32(netif.netmask)) ==
				   (IPADDR_BROADCAST & ~ip_addr.ip4_addr_get_u32(netif.netmask))))
			{
				/* => network broadcast address */
				return true;
			}
			else
			{
				return false;
			}
		}

		/** Checks if a netmask is valid (starting with ones, then only zeros)
		 *
		 * @param netmask the IPv4 netmask to check (in network byte order!)
		 * @return 1 if the netmask is valid, 0 if it is not
		 */
		public static bool ip4_addr_netmask_valid(uint netmask)
		{
			uint mask;
			uint nm_hostorder = lwip.lwip_htonl(netmask);

			/* first, check for the first zero */
			for (mask = 1U << 31; mask != 0; mask >>= 1)
			{
				if ((nm_hostorder & mask) == 0)
				{
					break;
				}
			}
			/* then check that there is no one */
			for (; mask != 0; mask >>= 1)
			{
				if ((nm_hostorder & mask) != 0)
				{
					/* there is a one after the first zero . invalid */
					return false;
				}
			}
			/* no one after the first zero . valid */
			return true;
		}

		/* Here for now until needed in other places in lwIP */
		public static bool in_range(char c, char lo, char up) { return (c >= lo && c <= up); }
		public static bool isprint(char c) { return in_range(c, (char)0x20, (char)0x7f); }
		public static bool isdigit(char c) { return in_range(c, (char)'0', (char)'9'); }
		public static bool isxdigit(char c) { return (isdigit(c) || in_range(c, (char)'a', (char)'f') || in_range(c, (char)'A', (char)'F')); }
		public static bool islower(char c) { return in_range(c, (char)'a', (char)'z'); }
		public static bool isspace(char c) { return (c == (char)' ' || c == (char)'\f' || c == (char)'\n' || c == (char)'\r' || c == (char)'\t' || c == (char)'\v'); }

		/**
		 * Ascii internet address interpretation routine.
		 * The value returned is in network order.
		 *
		 * @param cp IP address in ascii represenation (e.g. "127.0.0.1")
		 * @return ip address in network order
		 */
		public static uint ipaddr_addr(string cp)
		{
			ip_addr val = new ip_addr(0);

			if (ipaddr_aton(cp, val) != 0)
			{
				return ip_addr.ip4_addr_get_u32(val);
			}
			return (IPADDR_NONE);
		}

		/**
		 * Check whether "cp" is a valid ascii representation
		 * of an Internet address and convert to a binary address.
		 * Returns 1 if the address is valid, 0 if not.
		 * This replaces inet_addr, the return value from which
		 * cannot distinguish between failure and a local broadcast address.
		 *
		 * @param cp IP address in ascii represenation (e.g. "127.0.0.1")
		 * @param addr pointer to which to save the ip address in network order
		 * @return 1 if cp could be converted to addr, 0 on failure
		 */
		public static int ipaddr_aton(string cp, ip_addr addr)
		{
			uint val;
			byte @base;
			char c;
			uint[] parts = new uint[4];
			int pp = 0;
			int p = 0;

			cp += "\0";
			c = cp[0];
			for (;;)
			{
				/*
				 * Collect number up to ``.''.
				 * Values are specified as for C:
				 * 0x=hex, 0=octal, 1-9=decimal.
				 */
				if (!isdigit(c))
					return (0);
				val = 0;
				@base = 10;
				if (c == '0')
				{
					c = cp[++p];
					if (c == 'x' || c == 'X')
					{
						@base = 16;
						c = cp[++p];
					}
					else
						@base = 8;
				}
				for (;;)
				{
					if (isdigit(c))
					{
						val = (val * @base) + (uint)(c - (byte)'0');
						c = cp[++p];
					}
					else if (@base == 16 && isxdigit(c))
					{
						val = (val << 4) | (uint)(c + 10 - (islower(c) ? (byte)'a' : (byte)'A'));
						c = cp[++p];
					}
					else
						break;
				}
				if (c == '.')
				{
					/*
					 * Internet format:
					 *  a.b.c.d
					 *  a.b.c   (with c treated as 16 bits)
					 *  a.b (with b treated as 24 bits)
					 */
					if (pp >= 3)
					{
						return (0);
					}
					parts[pp++] = val;
					c = cp[++p];
				}
				else
					break;
			}
			/*
			 * Check for trailing characters.
			 */
			if (c != '\0' && !isspace(c))
			{
				return (0);
			}
			/*
			 * Concoct the address according to
			 * the number of parts specified.
			 */
			switch (pp + 1)
			{

				case 0:
					return (0);       /* initial nondigit */

				case 1:             /* a -- 32 bits */
					break;

				case 2:             /* a.b -- 8.24 bits */
					if (val > 0xffffffUL)
					{
						return (0);
					}
					val |= parts[0] << 24;
					break;

				case 3:             /* a.b.c -- 8.8.16 bits */
					if (val > 0xffff)
					{
						return (0);
					}
					val |= (parts[0] << 24) | (parts[1] << 16);
					break;

				case 4:             /* a.b.c.d -- 8.8.8.8 bits */
					if (val > 0xff)
					{
						return (0);
					}
					val |= (parts[0] << 24) | (parts[1] << 16) | (parts[2] << 8);
					break;
				default:
					lwip.LWIP_ASSERT("unhandled", false);
					break;
			}
			if (addr != null)
			{
				ip_addr.ip4_addr_set_u32(addr, lwip.lwip_htonl(val));
			}
			return (1);
		}

		/**
		 * Convert numeric IP address into decimal dotted ASCII representation.
		 * returns ptr to static buffer; not reentrant!
		 *
		 * @param addr ip address in network order to convert
		 * @return pointer to a global static (!) buffer that holds the ASCII
		 *         represenation of addr
		 */
		public static string ipaddr_ntoa(ip_addr addr)
		{
			char[] str = new char[16];
			return ipaddr_ntoa_r(addr, str);
		}

		/**
		 * Same as ipaddr_ntoa, but reentrant since a user-supplied buffer is used.
		 *
		 * @param addr ip address in network order to convert
		 * @param buf target buffer where the pointer is stored
		 * @param buflen length of buf
		 * @return either pointer to buf which now holds the ASCII
		 *         representation of addr or null if buf was too small
		 */
		public static string ipaddr_ntoa_r(ip_addr addr, char[] buf)
		{
			byte[] s_addr;
			char[] inv = new char[3];
			int rp;
			int ap;
			byte rem;
			byte n;
			byte i;
			int len = 0;

			s_addr = BitConverter.GetBytes(ip_addr.ip4_addr_get_u32(addr));

			rp = 0;
			ap = 0;
			for (n = 0; n < 4; n++)
			{
				i = 0;
				do
				{
					rem = (byte)(s_addr[ap] % 10);
					s_addr[ap] /= (byte)10;
					inv[i++] = (char)('0' + rem);
				} while (s_addr[ap] != 0);
				while ((i--) != 0)
				{
					if (len++ >= buf.Length)
					{
						return null;
					}
					buf[rp++] = inv[i];
				}
				if (len++ >= buf.Length)
				{
					return null;
				}
				buf[rp++] = '.';
				ap++;
			}
			buf[--rp] = '\0';
			return buf.ToString();
		}

		internal static string to_string(ip_addr ipaddr)
		{
			return String.Format("{0}.{1}.{2}.{3}",
				ipaddr != null ? ip_addr.ip4_addr1_16(ipaddr) : 0,
				ipaddr != null ? ip_addr.ip4_addr2_16(ipaddr) : 0,
				ipaddr != null ? ip_addr.ip4_addr3_16(ipaddr) : 0,
				ipaddr != null ? ip_addr.ip4_addr4_16(ipaddr) : 0);
		}
	}
}
