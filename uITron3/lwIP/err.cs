/**
 * @file
 * Error Management module
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
	public enum err_t
	{
		/* Definitions for error constants. */

		ERR_OK = 0,                     /* No error, everything OK. */
		ERR_MEM = -1,                   /* Out of memory error.     */
		ERR_BUF = -2,                   /* Buffer error.            */
		ERR_TIMEOUT = -3,               /* Timeout.                 */
		ERR_RTE = -4,                   /* Routing problem.         */
		ERR_INPROGRESS = -5,            /* Operation in progress    */
		ERR_VAL = -6,                   /* Illegal value.           */
		ERR_WOULDBLOCK = -7,            /* Operation would block.   */
		ERR_USE = -8,                   /* Address in use.          */
		ERR_ISCONN = -9,                /* Already connected.       */

		ERR_ABRT = -10,                 /* Connection aborted.      */
		ERR_RST = -11,                  /* Connection reset.        */
		ERR_CLSD = -12,                 /* Connection closed.       */
		ERR_CONN = -13,                 /* Not connected.           */

		ERR_ARG = -14,                  /* Illegal argument.        */

		ERR_IF = -15,                   /* Low-level netif error    */
	}

	public partial class lwip
	{
		public static bool ERR_IS_FATAL(err_t e) { return ((e) < err_t.ERR_ISCONN); }

		static string[] err_strerr = {
			"Ok.",                    /* err_t.ERR_OK          0  */
			"Out of memory error.",   /* err_t.ERR_MEM        -1  */
			"Buffer error.",          /* err_t.ERR_BUF        -2  */
			"Timeout.",               /* err_t.ERR_TIMEOUT    -3  */
			"Routing problem.",       /* err_t.ERR_RTE        -4  */
			"Operation in progress.", /* err_t.ERR_INPROGRESS -5  */
			"Illegal value.",         /* err_t.ERR_VAL        -6  */
			"Operation would block.", /* err_t.ERR_WOULDBLOCK -7  */
			"Address in use.",        /* err_t.ERR_USE        -8  */
			"Already connected.",     /* err_t.ERR_ISCONN     -9  */
			"Connection aborted.",    /* err_t.ERR_ABRT       -10 */
			"Connection reset.",      /* err_t.ERR_RST        -11 */
			"Connection closed.",     /* err_t.ERR_CLSD       -12 */
			"Not connected.",         /* err_t.ERR_CONN       -13 */
			"Illegal argument.",      /* err_t.ERR_ARG        -14 */
			"Low-level netif error.", /* err_t.ERR_IF         -15 */
		};

		/**
		 * Convert an lwip internal error to a pointer representation.
		 *
		 * @param err an lwip internal err_t
		 * @return a pointer representation for err
		 */
		public static string lwip_strerr(err_t err)
		{
			return err_strerr[-(int)err];
		}
	}
}
