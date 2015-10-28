/**
 * @file
 * Common functions used throughout the stack.
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
 * Author: Simon Goldschmidt
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public partial class lwip
	{
		/* These macros should be calculated by the preprocessor and are used
		   with compile-time constants only (so that there is no little-endian
		   overhead at runtime). */
		public static ushort PP_HTONS(ushort x) { return (ushort)(((x & 0xff) << 8) | ((x & 0xff00) >> 8)); }
		public static ushort PP_NTOHS(ushort x) { return lwip.PP_HTONS(x); }
		public static uint PP_HTONL(uint x)
		{
			return (((x & 0xff) << 24) |
				((x & 0xff00) << 8) |
				((x & 0xff0000U) >> 8) |
				((x & 0xff000000U) >> 24));
		}
		public static uint PP_NTOHL(uint x) { return lwip.PP_HTONL(x); }

		/**
		 * These are reference implementations of the byte swapping functions.
		 * Again with the aim of being simple, correct and fully portable.
		 * Byte swapping is the second thing you would want to optimize. You will
		 * need to port it to your architecture and in your cc.h:
		 * 
		 * #define LWIP_PLATFORM_BYTESWAP 1
		 * #define LWIP_PLATFORM_HTONS(x) <your_htons>
		 * #define LWIP_PLATFORM_HTONL(x) <your_htonl>
		 *
		 * Note lwip.lwip_ntohs() and ntohl() are merely references to the htonx counterparts.
		 */

		/**
		 * Convert an ushort from host- to network byte order.
		 *
		 * @param n ushort in host byte order
		 * @return n in network byte order
		 */
		public static ushort lwip_htons(ushort n)
		{
			return (ushort)(((n & 0xff) << 8) | ((n & 0xff00) >> 8));
		}

		/**
		 * Convert an ushort from network- to host byte order.
		 *
		 * @param n ushort in network byte order
		 * @return n in host byte order
		 */
		public static ushort lwip_ntohs(ushort n)
		{
			return lwip_htons(n);
		}

		/**
		 * Convert an uint from host- to network byte order.
		 *
		 * @param n uint in host byte order
		 * @return n in network byte order
		 */
		public static uint lwip_htonl(uint n)
		{
			return ((n & 0xff) << 24) |
			  ((n & 0xff00) << 8) |
			  ((n & 0xff0000U) >> 8) |
			  ((n & 0xff000000U) >> 24);
		}

		/**
		 * Convert an uint from network- to host byte order.
		 *
		 * @param n uint in network byte order
		 * @return n in host byte order
		 */
		public static uint lwip_ntohl(uint n)
		{
			return lwip_htonl(n);
		}
	}
}
