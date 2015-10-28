/**
 * @file
 * Incluse internet checksum functions.
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
#define LWIP_CHKSUM_ALGORITHM_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public partial class lwip
	{
		/** Swap the bytes in an ushort: much like htons() for little-endian */
#if !SWAP_BYTES_IN_WORD
#if LWIP_PLATFORM_BYTESWAP
		/* little endian and PLATFORM_BYTESWAP defined */
		public static ushort SWAP_BYTES_IN_WORD(ushort w) { return LWIP_PLATFORM_HTONSw; }
#else // LWIP_PLATFORM_BYTESWAP && (BYTE_ORDER == LITTLE_ENDIAN)
		/* can't use htons on big endian (or PLATFORM_BYTESWAP not defined)... */
		public static ushort SWAP_BYTES_IN_WORD(ushort w) { return (ushort)(((w & 0xff) << 8) | ((w & 0xff00) >> 8)); }
#endif // LWIP_PLATFORM_BYTESWAP && (BYTE_ORDER == LITTLE_ENDIAN)
#endif // SWAP_BYTES_IN_WORD

		/** Split an uint in two u16_ts and add them up */
#if !FOLD_U32T
		public static uint FOLD_U32T(uint u) { return ((u >> 16) + (u & 0x0000ffffU)); }
#endif

#if LWIP_CHECKSUM_ON_COPY
		/** Function-like macro: same as MEMCPY but returns the checksum of copied data
			as ushort */
		public static ushort LWIP_CHKSUM_COPY(pointer dst, pointer src, ushort len) { return lwip_chksum_copy(dst, src, len); }
#endif // LWIP_CHECKSUM_ON_COPY

		/* These are some reference implementations of the checksum algorithm, with the
		 * aim of being simple, correct and fully portable. Checksumming is the
		 * first thing you would want to optimize for your platform. If you create
		 * your own version, link it in and in your cc.h put:
		 * 
		 * #define LWIP_CHKSUM <your_checksum_routine> 
		 *
		 * Or you can select from the implementations below by defining
		 * LWIP_CHKSUM_ALGORITHM to 1, 2 or 3.
		 */
#if LWIP_CHKSUM_ALGORITHM_1 // Version #1
		/**
		 * lwip checksum
		 *
		 * @param dataptr points to start of data to be summed at any boundary
		 * @param len length of data to be summed
		 * @return host order (!) lwip checksum (non-inverted Internet sum) 
		 *
		 * @note accumulator size limits summable length to 64k
		 * @note host endianess is irrelevant (p3 RFC1071)
		 */
		private static ushort lwip_standard_chksum(pointer dataptr, ushort len)
		{
			uint acc;
			ushort src;
			int octetptr;

			acc = 0;
			/* dataptr may be at odd or even addresses */
			octetptr = 0;
			while (len > 1)
			{
				/* declare first octet as most significant
				   thus assume network order, ignoring host order */
				src = (ushort)(dataptr[octetptr] << 8);
				octetptr++;
				/* declare second octet as least significant */
				src |= dataptr[octetptr];
				octetptr++;
				acc += src;
				len -= 2;
			}
			if (len > 0)
			{
				/* accumulate remaining octet */
				src = (ushort)(dataptr[octetptr] << 8);
				acc += src;
			}
			/* add deferred carry bits */
			acc = (uint)((acc >> 16) + (acc & 0x0000ffffUL));
			if ((acc & 0xffff0000UL) != 0)
			{
				acc = (uint)((acc >> 16) + (acc & 0x0000ffffUL));
			}
			/* This maybe a little confusing: reorder sum using htons()
			   instead of lwip.lwip_ntohs() since it has a little less call overhead.
			   The caller must invert bits for Internet sum ! */
			return lwip.lwip_htons((ushort)acc);
		}
#endif

#if LWIP_CHKSUM_ALGORITHM_2 // Alternative version #2
		/*
		 * Curt McDowell
		 * Broadcom Corp.
		 * csm@broadcom.com
		 *
		 * IP checksum two bytes at a time with support for
		 * unaligned buffer.
		 * Works for len up to and including 0x20000.
		 * by Curt McDowell, Broadcom Corp. 12/08/2005
		 *
		 * @param dataptr points to start of data to be summed at any boundary
		 * @param len length of data to be summed
		 * @return host order (!) lwip checksum (non-inverted Internet sum) 
		 */
		private static ushort lwip_standard_chksum(pointer dataptr, ushort len)
		{
			int pb = 0, ps = 0;
			ushort t = 0;
			uint sum = 0;
			int odd = (pb & 1);

			/* Get aligned to ushort */
			if (odd != 0 && len > 0)
			{
				((int)t)[1] = pb++;
				len--;
			}

			/* Add the bulk of the data */
			ps = (ushort)(byte[])pb;
			while (len > 1)
			{
				sum += ps++;
				len -= 2;
			}

			/* Consume left-over byte, if any */
			if (len > 0)
			{
				((int)t)[0] = *(int)ps;
			}

			/* Add end bytes */
			sum += t;

			/* Fold 32-bit sum to 16 bits
			   calling this twice is propably faster than if statements... */
			sum = FOLD_U32T(sum);
			sum = FOLD_U32T(sum);

			/* Swap if alignment was odd */
			if (odd)
			{
				sum = SWAP_BYTES_IN_WORD(sum);
			}

			return (ushort)sum;
		}
#endif

#if LWIP_CHKSUM_ALGORITHM_3 // Alternative version #3
		/**
		 * An optimized checksum routine. Basically, it uses loop-unrolling on
		 * the checksum loop, treating the head and tail bytes specially, whereas
		 * the inner loop acts on 8 bytes at a time. 
		 *
		 * @arg start of buffer to be checksummed. May be an odd byte address.
		 * @len number of bytes in the buffer to be checksummed.
		 * @return host order (!) lwip checksum (non-inverted Internet sum) 
		 * 
		 * by Curt McDowell, Broadcom Corp. December 8th, 2005
		 */
		private static ushort lwip_standard_chksum(pointer dataptr, ushort len)
		{
			int pb = (int)dataptr;
			ushort ps, t = 0;
			uint pl;
			uint sum = 0, tmp;
			/* starts at odd byte address? */
			int odd = (pb & 1);

			if (odd && len > 0)
			{
				((int)t)[1] = pb++;
				len--;
			}

			ps = (ushort)pb;

			if ((ps & 3) && len > 1)
			{
				sum += ps++;
				len -= 2;
			}

			pl = (uint)ps;

			while (len > 7)
			{
				tmp = sum + pl++;          /* ping */
				if (tmp < sum)
				{
					tmp++;                    /* add back carry */
				}

				sum = tmp + pl++;          /* pong */
				if (sum < tmp)
				{
					sum++;                    /* add back carry */
				}

				len -= 8;
			}

			/* make room in upper bits */
			sum = FOLD_U32T(sum);

			ps = (ushort)pl;

			/* 16-bit aligned word remaining? */
			while (len > 1)
			{
				sum += ps++;
				len -= 2;
			}

			/* dangling tail byte remaining? */
			if (len > 0)
			{                /* include odd int */
				((int)t)[0] = *(int)ps;
			}

			sum += t;                     /* add end bytes */

			/* Fold 32-bit sum to 16 bits
			   calling this twice is propably faster than if statements... */
			sum = FOLD_U32T(sum);
			sum = FOLD_U32T(sum);

			if (odd)
			{
				sum = SWAP_BYTES_IN_WORD(sum);
			}

			return (ushort)sum;
		}
#endif

		/* lwip.inet_chksum_pseudo:
		 *
		 * Calculates the pseudo Internet checksum used by TCP and UDP for a pbuf chain.
		 * IP addresses are expected to be in network byte order.
		 *
		 * @param p chain of pbufs over that a checksum should be calculated (ip data part)
		 * @param src source ip address (used for checksum of pseudo header)
		 * @param dst destination ip address (used for checksum of pseudo header)
		 * @param proto ip protocol (used for checksum of pseudo header)
		 * @param proto_len length of the ip data part (used for checksum of pseudo header)
		 * @return checksum (as ushort) to be saved directly in the protocol header
		 */
		public static ushort inet_chksum_pseudo(pbuf p, ip_addr src, ip_addr dest,
			byte proto, ushort proto_len)
		{
			uint acc;
			uint addr;
			pbuf q;
			byte swapped;

			acc = 0;
			swapped = 0;
			/* iterate through all pbuf in chain */
			for (q = p; q != null; q = q.next)
			{
				lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): checksumming pbuf {0} (has next {1}) \n",
				  q, q.next);
				acc += LWIP_CHKSUM(q.payload, q.len);
				/*lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): unwrapped lwip_chksum()={0} \n", acc);*/
				/* just executing this next line is probably faster that the if statement needed
				   to check whether we really need to execute it, and does no harm */
				acc = FOLD_U32T(acc);
				if (q.len % 2 != 0)
				{
					swapped = (byte)(1 - swapped);
					acc = SWAP_BYTES_IN_WORD((ushort)acc);
				}
				/*lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): wrapped lwip_chksum()={0} \n", acc);*/
			}

			if (swapped != 0)
			{
				acc = SWAP_BYTES_IN_WORD((ushort)acc);
			}
			addr = ip_addr.ip4_addr_get_u32(src);
			acc += (addr & 0xffffU);
			acc += ((addr >> 16) & 0xffffU);
			addr = ip_addr.ip4_addr_get_u32(dest);
			acc += (addr & 0xffffU);
			acc += ((addr >> 16) & 0xffffU);
			acc += (uint)lwip.lwip_htons((ushort)proto);
			acc += (uint)lwip.lwip_htons(proto_len);

			/* Fold 32-bit sum to 16 bits
			   calling this twice is propably faster than if statements... */
			acc = FOLD_U32T(acc);
			acc = FOLD_U32T(acc);
			lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): pbuf chain lwip_chksum()={0}\n", acc);
			return (ushort)~(acc & 0xffffU);
		}

		/* lwip.inet_chksum_pseudo:
		 *
		 * Calculates the pseudo Internet checksum used by TCP and UDP for a pbuf chain.
		 * IP addresses are expected to be in network byte order.
		 *
		 * @param p chain of pbufs over that a checksum should be calculated (ip data part)
		 * @param src source ip address (used for checksum of pseudo header)
		 * @param dst destination ip address (used for checksum of pseudo header)
		 * @param proto ip protocol (used for checksum of pseudo header)
		 * @param proto_len length of the ip data part (used for checksum of pseudo header)
		 * @return checksum (as ushort) to be saved directly in the protocol header
		 */
		public static ushort inet_chksum_pseudo_partial(pbuf p,
			   ip_addr src, ip_addr dest,
			   byte proto, ushort proto_len, ushort chksum_len)
		{
			uint acc;
			uint addr;
			pbuf q;
			byte swapped;
			ushort chklen;

			acc = 0;
			swapped = 0;
			/* iterate through all pbuf in chain */
			for (q = p; (q != null) && (chksum_len > 0); q = q.next)
			{
				lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): checksumming pbuf {0} (has next {1}) \n",
					q, q.next);
				chklen = q.len;
				if (chklen > chksum_len)
				{
					chklen = chksum_len;
				}
				acc += LWIP_CHKSUM(q.payload, chklen);
				chksum_len -= chklen;
				lwip.LWIP_ASSERT("delete me", chksum_len < 0x7fff);
				/*lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): unwrapped lwip_chksum()={0} \n", acc);*/
				/* fold the upper bit down */
				acc = FOLD_U32T(acc);
				if (q.len % 2 != 0)
				{
					swapped = (byte)(1 - swapped);
					acc = SWAP_BYTES_IN_WORD((ushort)acc);
				}
				/*lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): wrapped lwip_chksum()={0} \n", acc);*/
			}

			if (swapped != 0)
			{
				acc = SWAP_BYTES_IN_WORD((ushort)acc);
			}
			addr = ip_addr.ip4_addr_get_u32(src);
			acc += (addr & 0xffffU);
			acc += ((addr >> 16) & 0xffffU);
			addr = ip_addr.ip4_addr_get_u32(dest);
			acc += (addr & 0xffffU);
			acc += ((addr >> 16) & 0xffffU);
			acc += (uint)lwip.lwip_htons((ushort)proto);
			acc += (uint)lwip.lwip_htons(proto_len);

			/* Fold 32-bit sum to 16 bits
			   calling this twice is propably faster than if statements... */
			acc = FOLD_U32T(acc);
			acc = FOLD_U32T(acc);
			lwip.LWIP_DEBUGF(opt.INET_DEBUG, "inet_chksum_pseudo(): pbuf chain lwip_chksum()={0}\n", acc);
			return (ushort)~(acc & 0xffffUL);
		}

		/* lwip.inet_chksum:
		 *
		 * Calculates the Internet checksum over a portion of memory. Used primarily for IP
		 * and ICMP.
		 *
		 * @param dataptr start of the buffer to calculate the checksum (no alignment needed)
		 * @param len length of the buffer to calculate the checksum
		 * @return checksum (as ushort) to be saved directly in the protocol header
		 */

		public static ushort inet_chksum(pointer dataptr, ushort len)
		{
			return (ushort)~LWIP_CHKSUM(dataptr, len);
		}

		/**
		 * Calculate a checksum over a chain of pbufs (without pseudo-header, much like
		 * lwip.inet_chksum only pbufs are used).
		 *
		 * @param p pbuf chain over that the checksum should be calculated
		 * @return checksum (as ushort) to be saved directly in the protocol header
		 */
		public static ushort inet_chksum_pbuf(pbuf p)
		{
			uint acc;
			pbuf q;
			byte swapped;

			acc = 0;
			swapped = 0;
			for (q = p; q != null; q = q.next)
			{
				acc += LWIP_CHKSUM(q.payload, q.len);
				acc = FOLD_U32T(acc);
				if (q.len % 2 != 0)
				{
					swapped = (byte)(1 - swapped);
					acc = SWAP_BYTES_IN_WORD((ushort)acc);
				}
			}

			if (swapped != 0)
			{
				acc = SWAP_BYTES_IN_WORD((ushort)acc);
			}
			return (ushort)~(acc & 0xffffUL);
		}

		/* These are some implementations for LWIP_CHKSUM_COPY, which copies data
		 * like MEMCPY but generates a checksum at the same time. Since this is a
		 * performance-sensitive function, you might want to create your own version
		 * in assembly targeted at your hardware by defining it in lwipopts.h:
		 *   #define LWIP_CHKSUM_COPY(dst, src, len) your_chksum_copy(dst, src, len)
		 */
#if LWIP_CHECKSUM_ON_COPY // Version #1
		/** Safe but slow: first call MEMCPY, then call LWIP_CHKSUM.
		 * For architectures with big caches, data might still be in cache when
		 * generating the checksum after copying.
		 */
		public static ushort lwip_chksum_copy(pointer dst, pointer src, ushort len)
		{
			opt.MEMCPY(dst, src, len);
			return LWIP_CHKSUM(dst, len);
		}
#endif // (LWIP_CHKSUM_COPY_ALGORITHM == 1)

		private static ushort LWIP_CHKSUM(pointer dst, ushort len)
		{
			return lwip_standard_chksum(dst, len);
		}
	}
}