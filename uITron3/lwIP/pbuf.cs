/**
 * @file
 * Packet buffer management
 *
 * Packets are built from the pbuf data structure. It supports dynamic
 * memory allocation for packet contents or can reference externally
 * managed packet contents both in RAM and ROM. Quick allocation for
 * incoming packets is provided through pools with fixed sized pbufs.
 *
 * A packet may span over multiple pbufs, chained as a singly linked
 * list. This is called a "pbuf chain".
 *
 * Multiple packets may be queued, also using this singly linked list.
 * This is called a "packet queue".
 * 
 * So, a packet queue consists of one or more pbuf chains, each of
 * which consist of one or more pbufs. CURRENTLY, PACKET QUEUES ARE
 * NOT SUPPORTED!!! Use helper structs to queue multiple packets.
 * 
 * The differences between a pbuf chain and a packet queue are very
 * precise but subtle. 
 *
 * The last pbuf of a packet has a .tot_len field that equals the
 * .len field. It can be found by traversing the list. If the last
 * pbuf of a packet has a .next field other than null, more packets
 * are on the queue.
 *
 * Therefore, looping through a pbuf of a single packet, has an
 * loop end condition (tot_len == p.len), NOT (next == null).
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
	public partial class pbuf : pointer
	{
		lwip lwip;

		public pbuf(lwip lwip, byte[] data, int offset)
			: base(data, offset)
		{
			this.lwip = lwip;
		}

		public pbuf(lwip lwip, byte[] data)
			: this(lwip, data, 0)
		{
		}

		public pbuf(lwip lwip, pointer data)
			: this(lwip, data.data, data.offset)
		{
		}

		/** Currently, the pbuf_custom code is only needed for one specific configuration
		 * of IP_FRAG */
		public const bool LWIP_SUPPORT_CUSTOM_PBUF = (opt.IP_FRAG != 0) && (opt.IP_FRAG_USES_STATIC_BUF == 0) && (opt.LWIP_NETIF_TX_SINGLE_PBUF == 0);

		public const int PBUF_TRANSPORT_HLEN = 20;
		public const int PBUF_IP_HLEN = 20;

		/* Initializes the pbuf module. This call is empty for now, but may not be in future. */
		public static void pbuf_init(lwip lwip) { }
	}

	public enum pbuf_layer
	{
		PBUF_TRANSPORT,
		PBUF_IP,
		PBUF_LINK,
		PBUF_RAW
	}

	public enum pbuf_type
	{
		PBUF_RAM, /* pbuf data is stored in RAM */
		PBUF_ROM, /* pbuf data is stored in ROM */
		PBUF_REF, /* pbuf comes from the pbuf pool */
		PBUF_POOL /* pbuf payload refers to RAM */
	}

	public partial class pbuf
	{
		/** indicates this packet's data should be immediately passed to the application */
		public const byte PBUF_FLAG_PUSH = (byte)0x01U;
		/** indicates this is a custom pbuf: lwip.pbuf_free and pbuf_header handle such a
			a pbuf differently */
		public const byte PBUF_FLAG_IS_CUSTOM = (byte)0x02U;
		/** indicates this pbuf is UDP multicast to be looped back */
		public const byte PBUF_FLAG_MCASTLOOP = (byte)0x04U;
		/** indicates this pbuf was received as link-level broadcast */
		public const byte PBUF_FLAG_LLBCAST = (byte)0x08U;
		/** indicates this pbuf was received as link-level multicast */
		public const byte PBUF_FLAG_LLMCAST = (byte)0x10U;
		/** indicates this pbuf includes a TCP FIN flag */
		public const byte PBUF_FLAG_TCP_FIN = (byte)0x20U;

		/** next pbuf in singly linked pbuf chain */
		public pbuf next;

		/** pointer to the actual data in the buffer */
		public pointer payload;

		/**
		 * total length of this buffer and all next buffers in chain
		 * belonging to the same packet.
		 *
		 * For non-queue packet chains this is the invariant:
		 * p.tot_len == p.len + (p.next? p.next.tot_len: 0)
		 */
		public ushort tot_len;

		/** length of this buffer */
		public ushort len;

		/** pbuf_type as byte instead of enum to save space */
		public pbuf_type type;

		/** misc flags */
		public byte flags;

		/**
		 * the reference count always equals the number of pointers
		 * that refer to this pbuf. This can be pointers from an application,
		 * the stack itself, or pbuf.next pointers from a chain.
		 */
		public ushort @ref;

		internal static readonly int SIZEOF_STRUCT_PBUF = lwip.LWIP_MEM_ALIGN_SIZE(pbuf.length);
		internal static readonly int PBUF_POOL_BUFSIZE_ALIGNED = lwip.LWIP_MEM_ALIGN_SIZE(opt.PBUF_POOL_BUFSIZE);
	}

	partial class lwip
	{
#if LWIP_TCP && TCP_QUEUE_OOSEQ
		/** Define this to 0 to prevent freeing ooseq pbufs when the pbuf_type.PBUF_POOL is empty */
#if !PBUF_POOL_FREE_OOSEQ
		public const int PBUF_POOL_FREE_OOSEQ = 1;
#endif // PBUF_POOL_FREE_OOSEQ
#if NO_SYS && PBUF_POOL_FREE_OOSEQ
		/** When not using sys_check_timeouts(), call PBUF_CHECK_FREE_OOSEQ()
			at regular intervals from main level to check if ooseq pbufs need to be
			freed! */
		public void PBUF_CHECK_FREE_OOSEQ()
		{
			do {
				if (pbuf_free_ooseq_pending != 0) {
					/* pbuf_alloc() reported pbuf_type.PBUF_POOL to be empty . try to free some 
					   ooseq queued pbufs now */
					pbuf_free_ooseq();
				}
			} while (false);
		}
#endif // NO_SYS && PBUF_POOL_FREE_OOSEQ
#endif // LWIP_TCP && TCP_QUEUE_OOSEQ
#if TCP_QUEUE_OOSEQ
		private void PBUF_POOL_FREE_OOSEQ_QUEUE_CALL()
		{
			do {
				if (tcpip.tcpip_callback_with_block(pbuf_free_ooseq_callback, null, 0) != err_t.ERR_OK) {
					sys.SYS_ARCH_PROTECT(sys.old_level);
					pbuf_free_ooseq_pending = 0;
					sys.SYS_ARCH_UNPROTECT(sys.old_level);
				}
			} while (false);
		}
#endif
		volatile byte pbuf_free_ooseq_pending;
		private void PBUF_POOL_IS_EMPTY() { pbuf_pool_is_empty(); }
#if TCP_QUEUE_OOSEQ
		/**
		 * Attempt to reclaim some memory from queued out-of-sequence TCP segments
		 * if we run out of pool pbufs. It's better to give priority to new packets
		 * if we're running out.
		 *
		 * This must be done in the correct thread context therefore this function
		 * can only be used with NO_SYS=0 and through tcpip_callback.
		 */
		private void pbuf_free_ooseq()
		{
			tcp_pcb_common pcbc;
			sys.SYS_ARCH_DECL_PROTECT(sys.old_level);

			sys.SYS_ARCH_PROTECT(sys.old_level);
			pbuf_free_ooseq_pending = 0;
			sys.SYS_ARCH_UNPROTECT(sys.old_level);

			for (pcbc = tcp.tcp_active_pcbs; null != pcbc; pcbc = pcbc.next) {
				tcp_pcb pcb = pcbc as tcp_pcb;
				if ((null != pcb) && (null != pcb.ooseq)) {
					/** Free the ooseq pbufs of one PCB only */
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_free_ooseq: freeing out-of-sequence pbufs\n");
					tcp.tcp_segs_free(pcb.ooseq);
					pcb.ooseq = null;
					return;
				}
			}
		}

#if TCP_QUEUE_OOSEQ
		/**
		 * Just a callback function for tcpip_timeout() that calls pbuf_free_ooseq().
		 */
		private void pbuf_free_ooseq_callback(object arg)
		{
			//LWIP_UNUSED_ARG(arg);
			pbuf_free_ooseq();
		}
#endif // !NO_SYS
#endif
		/** Queue a call to pbuf_free_ooseq if not already queued. */
		private void pbuf_pool_is_empty()
		{
#if !PBUF_POOL_FREE_OOSEQ_QUEUE_CALL
			sys.SYS_ARCH_DECL_PROTECT(sys.old_level);
			sys.SYS_ARCH_PROTECT(sys.old_level);
			pbuf_free_ooseq_pending = 1;
			sys.SYS_ARCH_UNPROTECT(sys.old_level);
#else // PBUF_POOL_FREE_OOSEQ_QUEUE_CALL
			byte queued;
			sys.SYS_ARCH_DECL_PROTECT(sys.old_level);
			sys.SYS_ARCH_PROTECT(sys.old_level);
			queued = pbuf_free_ooseq_pending;
			pbuf_free_ooseq_pending = 1;
			sys.SYS_ARCH_UNPROTECT(sys.old_level);

			if (queued == 0) {
				/* queue a call to pbuf_free_ooseq if not already queued */
				PBUF_POOL_FREE_OOSEQ_QUEUE_CALL();
			}
#endif // PBUF_POOL_FREE_OOSEQ_QUEUE_CALL
		}

		/**
		 * Allocates a pbuf of the given type (possibly a chain for pbuf_type.PBUF_POOL type).
		 *
		 * The actual memory allocated for the pbuf is determined by the
		 * layer at which the pbuf is allocated and the requested size
		 * (from the size parameter).
		 *
		 * @param layer flag to define header size
		 * @param length size of the pbuf's payload
		 * @param type this parameter decides how and where the pbuf
		 * should be allocated as follows:
		 *
		 * - pbuf_type.PBUF_RAM: buffer memory for pbuf is allocated as one large
		 *             chunk. This includes protocol headers as well.
		 * - pbuf_type.PBUF_ROM: no buffer memory is allocated for the pbuf, even for
		 *             protocol headers. Additional headers must be prepended
		 *             by allocating another pbuf and chain in to the front of
		 *             the ROM pbuf. It is assumed that the memory used is really
		 *             similar to ROM in that it is immutable and will not be
		 *             changed. Memory which is dynamic should generally not
		 *             be attached to pbuf_type.PBUF_ROM pbufs. Use pbuf_type.PBUF_REF instead.
		 * - pbuf_type.PBUF_REF: no buffer memory is allocated for the pbuf, even for
		 *             protocol headers. It is assumed that the pbuf is only
		 *             being used in a single thread. If the pbuf gets queued,
		 *             then pbuf_take should be called to copy the buffer.
		 * - pbuf_type.PBUF_POOL: the pbuf is allocated as a pbuf chain, with pbufs from
		 *              the pbuf pool that is allocated during pbuf_init().
		 *
		 * @return the allocated pbuf. If multiple pbufs where allocated, this
		 * is the first pbuf of a pbuf chain.
		 */
		public pbuf pbuf_alloc(pbuf_layer layer, ushort length, pbuf_type type)
		{
			pbuf p, q, r;
			ushort offset;
			int rem_len; /* remaining length */
			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_alloc(length={0})\n", length);

			/* determine header offset */
			switch (layer) {
			case pbuf_layer.PBUF_TRANSPORT:
				/* add room for transport (often TCP) layer header */
				offset = opt.PBUF_LINK_HLEN + pbuf.PBUF_IP_HLEN + pbuf.PBUF_TRANSPORT_HLEN;
				break;
			case pbuf_layer.PBUF_IP:
				/* add room for IP layer header */
				offset = opt.PBUF_LINK_HLEN + pbuf.PBUF_IP_HLEN;
				break;
			case pbuf_layer.PBUF_LINK:
				/* add room for link layer header */
				offset = opt.PBUF_LINK_HLEN;
				break;
			case pbuf_layer.PBUF_RAW:
				offset = 0;
				break;
			default:
				lwip.LWIP_ASSERT("pbuf_alloc: bad pbuf layer", false);
				return null;
			}

			switch (type) {
			case pbuf_type.PBUF_POOL:
				/* allocate head of pbuf chain into p */
				p = new pbuf(this, memp_malloc(mempb_t.MEMP_PBUF_POOL));
				lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_alloc: allocated pbuf {0}\n", p);
				if (p == null) {
					PBUF_POOL_IS_EMPTY();
					return null;
				}
				p.type = type;
				p.next = null;

				/* make the payload pointer point 'offset' bytes into pbuf data memory */
				p.payload = lwip.LWIP_MEM_ALIGN((p + (pbuf.SIZEOF_STRUCT_PBUF + offset)));
				lwip.LWIP_ASSERT("pbuf_alloc: pbuf p.payload properly aligned",
						(p.payload.offset % opt.MEM_ALIGNMENT) == 0);
				/* the total length of the pbuf chain is the requested size */
				p.tot_len = length;
				/* set the length of the first pbuf in the chain */
				p.len = Math.Min(length, (ushort)(pbuf.PBUF_POOL_BUFSIZE_ALIGNED - lwip.LWIP_MEM_ALIGN_SIZE(offset)));
				lwip.LWIP_ASSERT("check p.payload + p.len does not overflow pbuf",
							(p.payload + p.len <=
							 p + pbuf.SIZEOF_STRUCT_PBUF + pbuf.PBUF_POOL_BUFSIZE_ALIGNED));
				lwip.LWIP_ASSERT("PBUF_POOL_BUFSIZE must be bigger than opt.MEM_ALIGNMENT",
				  (pbuf.PBUF_POOL_BUFSIZE_ALIGNED - lwip.LWIP_MEM_ALIGN_SIZE(offset)) > 0);
				/* set reference count (needed here in case we fail) */
				p.@ref = 1;

				/* now allocate the tail of the pbuf chain */

				/* remember first pbuf for linkage in next iteration */
				r = p;
				/* remaining length to be allocated */
				rem_len = length - p.len;
				/* any remaining pbufs to be allocated? */
				while (rem_len > 0) {
					q = new pbuf(this, memp_malloc(mempb_t.MEMP_PBUF_POOL));
					if (q == null) {
						PBUF_POOL_IS_EMPTY();
						/* free chain so far allocated */
						pbuf_free(p);
						/* bail out unsuccesfully */
						return null;
					}
					q.type = type;
					q.flags = 0;
					q.next = null;
					/* make previous pbuf point to this pbuf */
					r.next = q;
					/* set total length of this pbuf and next in chain */
					lwip.LWIP_ASSERT("rem_len < max_u16_t", rem_len < 0xffff);
					q.tot_len = (ushort)rem_len;
					/* this pbuf length is pool size, unless smaller sized tail */
					q.len = Math.Min((ushort)rem_len, (ushort)pbuf.PBUF_POOL_BUFSIZE_ALIGNED);
					q.payload = (q + pbuf.SIZEOF_STRUCT_PBUF);
					lwip.LWIP_ASSERT("pbuf_alloc: pbuf q.payload properly aligned",
							(q.payload.offset % opt.MEM_ALIGNMENT) == 0);
					lwip.LWIP_ASSERT("check p.payload + p.len does not overflow pbuf",
								(p.payload + p.len <=
								 p + pbuf.SIZEOF_STRUCT_PBUF + pbuf.PBUF_POOL_BUFSIZE_ALIGNED));
					q.@ref = 1;
					/* calculate remaining length to be allocated */
					rem_len -= q.len;
					/* remember this pbuf for linkage in next iteration */
					r = q;
				}
				/* end of chain */
				/*r.next = null;*/

				break;
			case pbuf_type.PBUF_RAM:
				/* If pbuf is to be allocated in RAM, allocate memory for it. */
				p = new pbuf(this, mem_malloc(lwip.LWIP_MEM_ALIGN_SIZE((ushort)(pbuf.SIZEOF_STRUCT_PBUF + offset)) + lwip.LWIP_MEM_ALIGN_SIZE(length)));
				if (p == null) {
					return null;
				}
				/* Set up internal structure of the pbuf. */
				p.payload = lwip.LWIP_MEM_ALIGN((p + pbuf.SIZEOF_STRUCT_PBUF + offset));
				p.len = p.tot_len = length;
				p.next = null;
				p.type = type;

				lwip.LWIP_ASSERT("pbuf_alloc: pbuf.payload properly aligned",
					   (p.payload.offset % opt.MEM_ALIGNMENT) == 0);
				break;
			/* pbuf references existing (non-volatile static constant) ROM payload? */
			case pbuf_type.PBUF_ROM:
			/* pbuf references existing (externally allocated) RAM payload? */
			case pbuf_type.PBUF_REF:
				/* only allocate memory for the pbuf structure */
				p = new pbuf(this, memp_malloc(mempb_t.MEMP_PBUF));
				if (p == null) {
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
								"pbuf_alloc: Could not allocate MEMP_PBUF for PBUF_{0}.\n",
								(type == pbuf_type.PBUF_ROM) ? "ROM" : "REF");
					return null;
				}
				/* caller must set this field properly, afterwards */
				p.payload = null;
				p.len = p.tot_len = length;
				p.next = null;
				p.type = type;
				break;
			default:
				lwip.LWIP_ASSERT("pbuf_alloc: erroneous type", false);
				return null;
			}
			/* set reference count */
			p.@ref = 1;
			/* set flags */
			p.flags = 0;
			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_alloc(length={0}) == {1}\n", length, p);
			return p;
		}


		/**
		 * Shrink a pbuf chain to a desired length.
		 *
		 * @param p pbuf to shrink.
		 * @param new_len desired new length of pbuf chain
		 *
		 * Depending on the desired length, the first few pbufs in a chain might
		 * be skipped and left unchanged. The new last pbuf in the chain will be
		 * resized, and any remaining pbufs will be freed.
		 *
		 * @note If the pbuf is ROM/REF, only the .tot_len and .len fields are adjusted.
		 * @note May not be called on a packet queue.
		 *
		 * @note Despite its name, pbuf_realloc cannot grow the size of a pbuf (chain).
		 */
		public void pbuf_realloc(pbuf p, ushort new_len)
		{
			pbuf q;
			ushort rem_len; /* remaining length */
			int grow;

			lwip.LWIP_ASSERT("pbuf_realloc: p != null", p != null);
			lwip.LWIP_ASSERT("pbuf_realloc: sane p.type", p.type == pbuf_type.PBUF_POOL ||
						p.type == pbuf_type.PBUF_ROM ||
						p.type == pbuf_type.PBUF_RAM ||
						p.type == pbuf_type.PBUF_REF);

			/* desired length larger than current length? */
			if (new_len >= p.tot_len) {
				/* enlarging not yet supported */
				return;
			}

			/* the pbuf chain grows by (new_len - p.tot_len) bytes
			 * (which may be negative in case of shrinking) */
			grow = new_len - p.tot_len;

			/* first, step over any pbufs that should remain in the chain */
			rem_len = new_len;
			q = p;
			/* should this pbuf be kept? */
			while (rem_len > q.len) {
				/* decrease remaining length by pbuf length */
				rem_len -= q.len;
				/* decrease total length indicator */
				lwip.LWIP_ASSERT("grow < max_u16_t", grow < 0xffff);
				q.tot_len += (ushort)grow;
				/* proceed to next pbuf in chain */
				q = q.next;
				lwip.LWIP_ASSERT("pbuf_realloc: q != null", q != null);
			}
			/* we have now reached the new last pbuf (in q) */
			/* rem_len == desired length for pbuf q */

			/* shrink allocated memory for pbuf_type.PBUF_RAM */
			/* (other types merely adjust their length fields */
			if ((q.type == pbuf_type.PBUF_RAM) && (rem_len != q.len)) {
				/* reallocate and adjust the length of the pbuf that will be split */
				q = new pbuf(this, mem_trim(q, q.payload - q + rem_len));
				lwip.LWIP_ASSERT("mem_trim returned q == null", q != null);
			}
			/* adjust length fields for new last pbuf */
			q.len = rem_len;
			q.tot_len = q.len;

			/* any remaining pbufs in chain? */
			if (q.next != null) {
				/* free remaining pbufs in chain */
				pbuf_free(q.next);
			}
			/* q is last packet in chain */
			q.next = null;

		}

		/**
		 * Adjusts the payload pointer to hide or reveal headers in the payload.
		 *
		 * Adjusts the .payload pointer so that space for a header
		 * (dis)appears in the pbuf payload.
		 *
		 * The .payload, .tot_len and .len fields are adjusted.
		 *
		 * @param p pbuf to change the header size.
		 * @param header_size_increment Number of bytes to increment header size which
		 * increases the size of the pbuf. New space is on the front.
		 * (Using a negative value decreases the header size.)
		 * If hdr_size_inc is 0, this function does nothing and returns succesful.
		 *
		 * pbuf_type.PBUF_ROM and pbuf_type.PBUF_REF type buffers cannot have their sizes increased, so
		 * the call will fail. A check is made that the increase in header size does
		 * not move the payload pointer in front of the start of the buffer.
		 * @return non-zero on failure, zero on success.
		 *
		 */
		public static byte pbuf_header(pbuf p, short header_size_increment)
		{
			pbuf_type type;
			pointer payload;
			short increment_magnitude;

			lwip.LWIP_ASSERT("p != null", p != null);
			if ((header_size_increment == 0) || (p == null)) {
				return 0;
			}

			if (header_size_increment < 0) {
				increment_magnitude = (short)-header_size_increment;
				/* Check that we aren't going to move off the end of the pbuf */
				if (lwip.LWIP_ERROR("increment_magnitude <= p.len", (increment_magnitude <= p.len))) return 1;
			}
			else {
				increment_magnitude = header_size_increment;
#if false
				/* Can't assert these as some callers speculatively call
				   pbuf_header() to see if it's OK.  Will return 1 below instead. */
				/* Check that we've got the correct type of pbuf to work with */
				lwip.LWIP_ASSERT("p.type == pbuf_type.PBUF_RAM || p.type == pbuf_type.PBUF_POOL", 
							p.type == pbuf_type.PBUF_RAM || p.type == pbuf_type.PBUF_POOL);
				/* Check that we aren't going to move off the beginning of the pbuf */
				lwip.LWIP_ASSERT("p.payload - increment_magnitude >= p + SIZEOF_STRUCT_PBUF",
							p.payload - increment_magnitude >= p + SIZEOF_STRUCT_PBUF);
#endif
			}

			type = p.type;
			/* remember current payload pointer */
			payload = p.payload;

			/* pbuf types containing payloads? */
			if (type == pbuf_type.PBUF_RAM || type == pbuf_type.PBUF_POOL) {
				/* set new payload pointer */
				p.payload = p.payload - header_size_increment;
				/* boundary check fails? */
				if (p.payload < p + pbuf.SIZEOF_STRUCT_PBUF) {
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
					  "pbuf_header: failed as {0} < {1} (not enough space for new header size)\n",
					  p.payload, (p + 1));
					/* restore old payload pointer */
					p.payload = payload;
					/* bail out unsuccesfully */
					return 1;
				}
				/* pbuf types refering to external payloads? */
			}
			else if (type == pbuf_type.PBUF_REF || type == pbuf_type.PBUF_ROM) {
				/* hide a header in the payload? */
				if ((header_size_increment < 0) && (increment_magnitude <= p.len)) {
					/* increase payload pointer */
					p.payload = p.payload - header_size_increment;
				}
				else {
					/* cannot expand payload to front (yet!)
					 * bail out unsuccesfully */
					return 1;
				}
			}
			else {
				/* Unknown type */
				lwip.LWIP_ASSERT("bad pbuf type", false);
				return 1;
			}
			/* modify pbuf length fields */
			p.len += (ushort)header_size_increment;
			p.tot_len += (ushort)header_size_increment;

			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_header: old {0} new {1} ({2})\n",
				payload, p.payload, header_size_increment);

			return 0;
		}

		/**
		 * Dereference a pbuf chain or queue and deallocate any no-longer-used
		 * pbufs at the head of this chain or queue.
		 *
		 * Decrements the pbuf reference count. If it reaches zero, the pbuf is
		 * deallocated.
		 *
		 * For a pbuf chain, this is repeated for each pbuf in the chain,
		 * up to the first pbuf which has a non-zero reference count after
		 * decrementing. So, when all reference counts are one, the whole
		 * chain is free'd.
		 *
		 * @param p The pbuf (chain) to be dereferenced.
		 *
		 * @return the number of pbufs that were de-allocated
		 * from the head of the chain.
		 *
		 * @note MUST NOT be called on a packet queue (Not verified to work yet).
		 * @note the reference counter of a pbuf equals the number of pointers
		 * that refer to the pbuf (or into the pbuf).
		 *
		 * @internal examples:
		 *
		 * Assuming existing chains a.b.c with the following reference
		 * counts, calling lwip.pbuf_free(a) results in:
		 * 
		 * 1.2.3 becomes ...1.3
		 * 3.3.3 becomes 2.3.3
		 * 1.1.2 becomes ......1
		 * 2.1.1 becomes 1.1.1
		 * 1.1.1 becomes .......
		 *
		 */
		public byte pbuf_free(pbuf p)
		{
			pbuf_type type;
			pbuf q;
			byte count;

			if (p == null) {
				lwip.LWIP_ASSERT("p != null", p != null);
				/* if assertions are disabled, proceed with debug output */
				lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS,
				  ("pbuf_free(p == null) was called.\n"));
				return 0;
			}
			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_free({0})\n", p);

			//PERF_START;

			lwip.LWIP_ASSERT("pbuf_free: sane type",
			  p.type == pbuf_type.PBUF_RAM || p.type == pbuf_type.PBUF_ROM ||
			  p.type == pbuf_type.PBUF_REF || p.type == pbuf_type.PBUF_POOL);

			count = 0;
			/* de-allocate all consecutive pbufs from the head of the chain that
			 * obtain a zero reference count after decrementing*/
			while (p != null) {
				ushort @ref;
				sys.SYS_ARCH_DECL_PROTECT(sys.old_level);
				/* Since decrementing @ref cannot be guaranteed to be a single machine operation
				 * we must protect it. We put the new @ref into a local variable to prevent
				 * further protection. */
				sys.SYS_ARCH_PROTECT(sys.old_level);
				/* all pbufs in a chain are referenced at least once */
				lwip.LWIP_ASSERT("pbuf_free: p.@ref > 0", p.@ref > 0);
				/* decrease reference count (number of pointers to pbuf) */
				@ref = --(p.@ref);
				sys.SYS_ARCH_UNPROTECT(sys.old_level);
				/* this pbuf is no longer referenced to? */
				if (@ref == 0) {
					/* remember next pbuf in chain for next iteration */
					q = p.next;
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_free: deallocating {0}\n", p);
					type = p.type;
#if LWIP_SUPPORT_CUSTOM_PBUF
					/* is this a custom pbuf? */
					if ((p.flags & PBUF_FLAG_IS_CUSTOM) != 0) {
						pbuf_custom pc = (pbuf_custom)p;
						lwip.LWIP_ASSERT("pc.custom_free_function != null", pc.custom_free_function != null);
						pc.custom_free_function(p);
					} else
#endif // LWIP_SUPPORT_CUSTOM_PBUF
					{
						/* is this a pbuf from the pool? */
						if (type == pbuf_type.PBUF_POOL) {
							memp_free(mempb_t.MEMP_PBUF_POOL, p);
							/* is this a ROM or RAM referencing pbuf? */
						}
						else if (type == pbuf_type.PBUF_ROM || type == pbuf_type.PBUF_REF) {
							memp_free(mempb_t.MEMP_PBUF, p);
							/* type == pbuf_type.PBUF_RAM */
						}
						else {
							mem_free(p);
						}
					}
					count++;
					/* proceed to next pbuf */
					p = q;
					/* p.@ref > 0, this pbuf is still referenced to */
					/* (and so the remaining pbufs in chain as well) */
				}
				else {
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_free: {0} has @ref {1}, ending here.\n", p, @ref);
					/* stop walking through the chain */
					p = null;
				}
			}
			//PERF_STOP("pbuf_free");
			/* return number of de-allocated pbufs */
			return count;
		}

		/**
		 * Count number of pbufs in a chain
		 *
		 * @param p first pbuf of chain
		 * @return the number of pbufs in a chain
		 */

		public static byte pbuf_clen(pbuf p)
		{
			byte len;

			len = 0;
			while (p != null) {
				++len;
				p = p.next;
			}
			return len;
		}

		/**
		 * Increment the reference count of the pbuf.
		 *
		 * @param p pbuf to increase reference counter of
		 *
		 */
		public void pbuf_ref(pbuf p)
		{
			sys.SYS_ARCH_DECL_PROTECT(sys.old_level);
			/* pbuf given? */
			if (p != null) {
				sys.SYS_ARCH_PROTECT(sys.old_level);
				++(p.@ref);
				sys.SYS_ARCH_UNPROTECT(sys.old_level);
			}
		}

		/**
		 * Concatenate two pbufs (each may be a pbuf chain) and take over
		 * the caller's reference of the tail pbuf.
		 * 
		 * @note The caller MAY NOT reference the tail pbuf afterwards.
		 * Use pbuf_chain() for that purpose.
		 * 
		 * @see pbuf_chain()
		 */
		public static void pbuf_cat(pbuf h, pbuf t)
		{
			pbuf p;

			if (lwip.LWIP_ERROR("(h != null) && (t != null) (programmer violates API)",
					   ((h != null) && (t != null))))
				return;

			/* proceed to last pbuf of chain */
			for (p = h; p.next != null; p = p.next) {
				/* add total length of second chain to all totals of first chain */
				p.tot_len += t.tot_len;
			}
			/* { p is last pbuf of first h chain, p.next == null } */
			lwip.LWIP_ASSERT("p.tot_len == p.len (of last pbuf in chain)", p.tot_len == p.len);
			lwip.LWIP_ASSERT("p.next == null", p.next == null);
			/* add total length of second chain to last pbuf total of first chain */
			p.tot_len += t.tot_len;
			/* chain last pbuf of head (p) with first of tail (t) */
			p.next = t;
			/* p.next now references t, but the caller will drop its reference to t,
			 * so netto there is no change to the reference count of t.
			 */
		}

		/**
		 * Chain two pbufs (or pbuf chains) together.
		 * 
		 * The caller MUST call lwip.pbuf_free(t) once it has stopped
		 * using it. Use pbuf_cat() instead if you no longer use t.
		 * 
		 * @param h head pbuf (chain)
		 * @param t tail pbuf (chain)
		 * @note The pbufs MUST belong to the same packet.
		 * @note MAY NOT be called on a packet queue.
		 *
		 * The .tot_len fields of all pbufs of the head chain are adjusted.
		 * The .next field of the last pbuf of the head chain is adjusted.
		 * The .@ref field of the first pbuf of the tail chain is adjusted.
		 *
		 */
		public void pbuf_chain(pbuf h, pbuf t)
		{
			pbuf_cat(h, t);
			/* t is now referenced by h */
			pbuf_ref(t);
			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_chain: {0} references {1}\n", h, t);
		}

		/**
		 * Dechains the first pbuf from its succeeding pbufs in the chain.
		 *
		 * Makes p.tot_len field equal to p.len.
		 * @param p pbuf to dechain
		 * @return remainder of the pbuf chain, or null if it was de-allocated.
		 * @note May not be called on a packet queue.
		 */
		public pbuf pbuf_dechain(pbuf p)
		{
			pbuf q;
			byte tail_gone = 1;
			/* tail */
			q = p.next;
			/* pbuf has successor in chain? */
			if (q != null) {
				/* assert tot_len invariant: (p.tot_len == p.len + (p.next? p.next.tot_len: 0) */
				lwip.LWIP_ASSERT("p.tot_len == p.len + q.tot_len", q.tot_len == p.tot_len - p.len);
				/* enforce invariant if assertion is disabled */
				q.tot_len = (ushort)(p.tot_len - p.len);
				/* decouple pbuf from remainder */
				p.next = null;
				/* total length of pbuf p is its own length only */
				p.tot_len = p.len;
				/* q is no longer referenced by p, free it */
				lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_dechain: unreferencing {0}\n", q);
				tail_gone = pbuf_free(q);
				if (tail_gone > 0) {
					lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE,
								"pbuf_dechain: deallocated {0} (as it is no longer referenced)\n", q);
				}
				/* return remaining tail or null if deallocated */
			}
			/* assert tot_len invariant: (p.tot_len == p.len + (p.next? p.next.tot_len: 0) */
			lwip.LWIP_ASSERT("p.tot_len == p.len", p.tot_len == p.len);
			return ((tail_gone > 0) ? null : q);
		}

		/**
		 *
		 * Create pbuf_type.PBUF_RAM copies of pbufs.
		 *
		 * Used to queue packets on behalf of the lwIP stack, such as
		 * ARP based queueing.
		 *
		 * @note You MUST explicitly use p = pbuf_take(p);
		 *
		 * @note Only one packet is copied, no packet queue!
		 *
		 * @param p_to pbuf destination of the copy
		 * @param p_from pbuf source of the copy
		 *
		 * @return err_t.ERR_OK if pbuf was copied
		 *         err_t.ERR_ARG if one of the pbufs is null or p_to is not big
		 *                 enough to hold p_from
		 */
		public static err_t pbuf_copy(pbuf p_to, pbuf p_from)
		{
			ushort offset_to = 0, offset_from = 0, len;

			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_copy({0}, {1})\n",
			  p_to, p_from);

			/* is the target big enough to hold the source? */
			if (lwip.LWIP_ERROR("pbuf_copy: target not big enough to hold source", ((p_to != null) &&
					   (p_from != null) && (p_to.tot_len >= p_from.tot_len))))
				return err_t.ERR_ARG;

			/* iterate through pbuf chain */
			do {
				/* copy one part of the original chain */
				if ((p_to.len - offset_to) >= (p_from.len - offset_from)) {
					/* complete current p_from fits into current p_to */
					len = (ushort)(p_from.len - offset_from);
				}
				else {
					/* current p_from does not fit into current p_to */
					len = (ushort)(p_to.len - offset_to);
				}
				opt.MEMCPY(p_to.payload + offset_to, p_from.payload + offset_from, len);
				offset_to += len;
				offset_from += len;
				lwip.LWIP_ASSERT("offset_to <= p_to.len", offset_to <= p_to.len);
				lwip.LWIP_ASSERT("offset_from <= p_from.len", offset_from <= p_from.len);
				if (offset_from >= p_from.len) {
					/* on to next p_from (if any) */
					offset_from = 0;
					p_from = p_from.next;
				}
				if (offset_to == p_to.len) {
					/* on to next p_to (if any) */
					offset_to = 0;
					p_to = p_to.next;
					if (lwip.LWIP_ERROR("p_to != null", (p_to != null) || (p_from == null))) return err_t.ERR_ARG;
				}

				if ((p_from != null) && (p_from.len == p_from.tot_len)) {
					/* don't copy more than one packet! */
					if (lwip.LWIP_ERROR("pbuf_copy() does not allow packet queues!\n",
						(p_from.next == null)))
						return err_t.ERR_VAL;
				}
				if ((p_to != null) && (p_to.len == p_to.tot_len)) {
					/* don't copy more than one packet! */
					if (lwip.LWIP_ERROR("pbuf_copy() does not allow packet queues!\n",
						(p_to.next == null)))
						return err_t.ERR_VAL;
				}
			} while (p_from != null);
			lwip.LWIP_DEBUGF(opt.PBUF_DEBUG | lwip.LWIP_DBG_TRACE, "pbuf_copy: end of chain reached.\n");
			return err_t.ERR_OK;
		}

		/**
		 * Copy (part of) the contents of a packet buffer
		 * to an application supplied buffer.
		 *
		 * @param buf the pbuf from which to copy data
		 * @param dataptr the application supplied buffer
		 * @param len length of data to copy (dataptr must be big enough). No more 
		 * than buf.tot_len will be copied, irrespective of len
		 * @param offset offset into the packet buffer from where to begin copying len bytes
		 * @return the number of bytes copied, or 0 on failure
		 */
		public static ushort pbuf_copy_partial(pbuf buf, pointer dataptr, ushort len, ushort offset)
		{
			pbuf p;
			ushort left;
			ushort buf_copy_len;
			ushort copied_total = 0;

			if (lwip.LWIP_ERROR("pbuf_copy_partial: invalid buf", (buf != null))) return 0;
			if (lwip.LWIP_ERROR("pbuf_copy_partial: invalid dataptr", (dataptr != null))) return 0;

			left = 0;

			if ((buf == null) || (dataptr == null)) {
				return 0;
			}

			/* Note some systems use byte copy if dataptr or one of the pbuf payload pointers are unaligned. */
			for (p = buf; len != 0 && p != null; p = p.next) {
				if ((offset != 0) && (offset >= p.len)) {
					/* don't copy from this buffer . on to the next */
					offset -= p.len;
				}
				else {
					/* copy from this buffer. maybe only partially. */
					buf_copy_len = (ushort)(p.len - offset);
					if (buf_copy_len > len)
						buf_copy_len = len;
					/* copy the necessary parts of the buffer */
					opt.MEMCPY(dataptr + left, p.payload + offset, buf_copy_len);
					copied_total += buf_copy_len;
					left += buf_copy_len;
					len -= buf_copy_len;
					offset = 0;
				}
			}
			return copied_total;
		}

		/**
		 * Copy application supplied data into a pbuf.
		 * This function can only be used to copy the equivalent of buf.tot_len data.
		 *
		 * @param buf pbuf to fill with data
		 * @param dataptr application supplied data buffer
		 * @param len length of the application supplied data buffer
		 *
		 * @return err_t.ERR_OK if successful, err_t.ERR_MEM if the pbuf is not big enough
		 */
		public static err_t pbuf_take(pbuf buf, pointer dataptr, ushort len)
		{
			pbuf p;
			ushort buf_copy_len;
			ushort total_copy_len = len;
			ushort copied_total = 0;

			if (lwip.LWIP_ERROR("pbuf_take: invalid buf", (buf != null))) return 0;
			if (lwip.LWIP_ERROR("pbuf_take: invalid dataptr", (dataptr != null))) return 0;

			if ((buf == null) || (dataptr == null) || (buf.tot_len < len)) {
				return err_t.ERR_ARG;
			}

			/* Note some systems use byte copy if dataptr or one of the pbuf payload pointers are unaligned. */
			for (p = buf; total_copy_len != 0; p = p.next) {
				lwip.LWIP_ASSERT("pbuf_take: invalid pbuf", p != null);
				buf_copy_len = total_copy_len;
				if (buf_copy_len > p.len) {
					/* this pbuf cannot hold all remaining data */
					buf_copy_len = p.len;
				}
				/* copy the necessary parts of the buffer */
				opt.MEMCPY(p.payload, dataptr + copied_total, buf_copy_len);
				total_copy_len -= buf_copy_len;
				copied_total += buf_copy_len;
			}
			lwip.LWIP_ASSERT("did not copy all data", total_copy_len == 0 && copied_total == len);
			return err_t.ERR_OK;
		}

		/**
		 * Creates a single pbuf out of a queue of pbufs.
		 *
		 * @remark: Either the source pbuf 'p' is freed by this function or the original
		 *          pbuf 'p' is returned, therefore the caller has to check the result!
		 *
		 * @param p the source pbuf
		 * @param layer pbuf_layer of the new pbuf
		 *
		 * @return a new, single pbuf (p.next is null)
		 *         or the old pbuf if allocation fails
		 */
		public pbuf pbuf_coalesce(pbuf p, pbuf_layer layer)
		{
			pbuf q;
			err_t err;
			if (p.next == null) {
				return p;
			}
			q = pbuf_alloc(layer, p.tot_len, pbuf_type.PBUF_RAM);
			if (q == null) {
				/* @todo: what do we do now? */
				return p;
			}
			err = pbuf_copy(q, p);
			lwip.LWIP_ASSERT("pbuf_copy failed", err == err_t.ERR_OK);
			pbuf_free(p);
			return q;
		}

#if LWIP_CHECKSUM_ON_COPY
		/**
		 * Copies data into a single pbuf (not into a pbuf queue!) and updates
		 * the checksum while copying
		 *
		 * @param p the pbuf to copy data into
		 * @param start_offset offset of p.payload where to copy the data to
		 * @param dataptr data to copy into the pbuf
		 * @param len length of data to copy into the pbuf
		 * @param chksum pointer to the checksum which is updated
		 * @return err_t.ERR_OK if successful, another error if the data does not fit
		 *         within the (first) pbuf (no pbuf queues!)
		 */
		public static err_t pbuf_fill_chksum(pbuf p, ushort start_offset, pointer dataptr,
						 ushort len, ref ushort chksum)
		{
			uint acc;
			ushort copy_chksum;
			pointer dst_ptr;
			lwip.LWIP_ASSERT("p != null", p != null);
			lwip.LWIP_ASSERT("dataptr != null", dataptr != null);
			//lwip.LWIP_ASSERT("chksum != null", chksum != null);
			lwip.LWIP_ASSERT("len != 0", len != 0);

			if ((start_offset >= p.len) || (start_offset + len > p.len)) {
				return err_t.ERR_ARG;
			}

			dst_ptr = ((pointer)p.payload) + start_offset;
			copy_chksum = lwip.LWIP_CHKSUM_COPY(dst_ptr, dataptr, len);
			if ((start_offset & 1) != 0) {
				copy_chksum = lwip.SWAP_BYTES_IN_WORD(copy_chksum);
			}
			acc = chksum;
			acc += copy_chksum;
			chksum = (ushort)lwip.FOLD_U32T(acc);
			return err_t.ERR_OK;
		}
#endif // LWIP_CHECKSUM_ON_COPY

		/** Get one byte from the specified position in a pbuf
		 * WARNING: returns zero for offset >= p.tot_len
		 *
		 * @param p pbuf to parse
		 * @param offset offset into p of the byte to return
		 * @return byte at an offset into p OR ZERO IF 'offset' >= p.tot_len
		 */
		public static byte pbuf_get_at(pbuf p, ushort offset)
		{
			ushort copy_from = offset;
			pbuf q = p;

			/* get the correct pbuf */
			while ((q != null) && (q.len <= copy_from)) {
				copy_from -= q.len;
				q = q.next;
			}
			/* return requested data if pbuf is OK */
			if ((q != null) && (q.len > copy_from)) {
				return (q.payload)[copy_from];
			}
			return 0;
		}

		/** Compare pbuf contents at specified offset with memory s2, both of length n
		 *
		 * @param p pbuf to compare
		 * @param offset offset into p at wich to start comparing
		 * @param s2 buffer to compare
		 * @param n length of buffer to compare
		 * @return zero if equal, nonzero otherwise
		 *         (0xffff if p is too short, diffoffset+1 otherwise)
		 */
		public static ushort pbuf_memcmp(pbuf p, ushort offset, pointer s2, ushort n)
		{
			ushort start = offset;
			pbuf q = p;

			/* get the correct pbuf */
			while ((q != null) && (q.len <= start)) {
				start -= q.len;
				q = q.next;
			}
			/* return requested data if pbuf is OK */
			if ((q != null) && (q.len > start)) {
				ushort i;
				for (i = 0; i < n; i++) {
					byte a = pbuf_get_at(q, (ushort)(start + i));
					byte b = (s2)[i];
					if (a != b) {
						return (ushort)(i + 1);
					}
				}
				return 0;
			}
			return 0xffff;
		}

		/** Find occurrence of mem (with length mem_len) in pbuf p, starting at offset
		 * start_offset.
		 *
		 * @param p pbuf to search, maximum length is 0xFFFE since 0xFFFF is used as
		 *        return value 'not found'
		 * @param mem search for the contents of this buffer
		 * @param mem_len length of 'mem'
		 * @param start_offset offset into p at which to start searching
		 * @return 0xFFFF if substr was not found in p or the index where it was found
		 */
		public static ushort pbuf_memfind(pbuf p, pointer mem, ushort mem_len, ushort start_offset)
		{
			ushort i;
			ushort max = (ushort)(p.tot_len - mem_len);
			if (p.tot_len >= mem_len + start_offset) {
				for (i = start_offset; i <= max;) {
					ushort plus = pbuf_memcmp(p, i, mem, mem_len);
					if (plus == 0) {
						return i;
					}
					else {
						i += plus;
					}
				}
			}
			return 0xFFFF;
		}

		/** Find occurrence of substr with length substr_len in pbuf p, start at offset
		 * start_offset
		 * WARNING: in contrast to strstr(), this one does not stop at the first \0 in
		 * the pbuf/source pointer!
		 *
		 * @param p pbuf to search, maximum length is 0xFFFE since 0xFFFF is used as
		 *        return value 'not found'
		 * @param substr pointer to search for in p, maximum length is 0xFFFE
		 * @return 0xFFFF if substr was not found in p or the index where it was found
		 */
		public static ushort pbuf_strstr(pbuf p, pointer substr)
		{
			int substr_len;
			if ((substr == null) || (substr[0] == 0) || (p.tot_len == 0xFFFF)) {
				return 0xFFFF;
			}
			substr_len = pointer.strlen(substr);
			if (substr_len >= 0xFFFF) {
				return 0xFFFF;
			}
			return pbuf_memfind(p, substr, (ushort)substr_len, 0);
		}
	}
}