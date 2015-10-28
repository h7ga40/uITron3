﻿/*
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
	public partial class lwip
	{
		/** lower two bits indicate debug level
		 * - 0 all
		 * - 1 warning
		 * - 2 serious
		 * - 3 severe
		 */
		public const int LWIP_DBG_LEVEL_ALL = 0x00;
		public const int LWIP_DBG_LEVEL_OFF = LWIP_DBG_LEVEL_ALL; /* compatibility define only */
		public const int LWIP_DBG_LEVEL_WARNING = 0x01; /* bad checksums, dropped packets, ... */
		public const int LWIP_DBG_LEVEL_SERIOUS = 0x02; /* memory allocation failures, ... */
		public const int LWIP_DBG_LEVEL_SEVERE = 0x03;
		public const int LWIP_DBG_MASK_LEVEL = 0x03;

		/** flag for LWIP_DEBUGF to enable that debug message */
		public const uint LWIP_DBG_ON = 0x80U;
		/** flag for LWIP_DEBUGF to disable that debug message */
		public const uint LWIP_DBG_OFF = 0x00U;

		/** flag for LWIP_DEBUGF indicating a tracing message (to follow program flow) */
		public const uint LWIP_DBG_TRACE = 0x40U;
		/** flag for LWIP_DEBUGF indicating a state debug message (to follow module states) */
		public const uint LWIP_DBG_STATE = 0x20U;
		/** flag for LWIP_DEBUGF indicating newly added code, not thoroughly tested yet */
		public const uint LWIP_DBG_FRESH = 0x10U;
		/** flag for LWIP_DEBUGF to halt after printing this debug message */
		public const uint LWIP_DBG_HALT = 0x08U;


		internal static void LWIP_ASSERT(string text, bool cond)
		{
			System.Diagnostics.Debug.Assert(cond, text);
		}

		internal static void LWIP_DEBUGF(uint flag, string format, params object[] args)
		{
			System.Diagnostics.Debug.Write(String.Format(format, args));
		}

		internal static bool LWIP_ERROR(string text, bool cond)
		{
			System.Diagnostics.Debug.Assert(cond, text);
			return !cond;
		}
	}
}
