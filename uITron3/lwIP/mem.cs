/**
 * @file
 * Dynamic memory manager
 *
 * This is a lightweight replacement for the standard C library malloc().
 *
 * If you want to use the standard C library malloc() instead, define
 * MEM_LIBC_MALLOC to 1 in your lwipopts.h
 *
 * To let mem_malloc() use pools (prevents fragmentation and is much faster than
 * a heap but might waste some memory), define MEM_USE_POOLS to 1, define
 * MEM_USE_CUSTOM_POOLS to 1 and create a file "lwippools.h" that includes a list
 * of pools like this (more pools can be added between _START and _END):
 *
 * Define three pools with sizes 256, 512, and 1512 bytes
 * LWIP_MALLOC_MEMPOOL_START
 * LWIP_MALLOC_MEMPOOL(20, 256)
 * LWIP_MALLOC_MEMPOOL(10, 512)
 * LWIP_MALLOC_MEMPOOL(5, 1512)
 * LWIP_MALLOC_MEMPOOL_END
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
 *         Simon Goldschmidt
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
	public partial class lwip
	{
		/** Calculate memory size for an aligned buffer - returns the next highest
		 * multiple of opt.MEM_ALIGNMENT (e.g. lwip.LWIP_MEM_ALIGN_SIZE(3) and
		 * lwip.LWIP_MEM_ALIGN_SIZE(4) will both yield 4 for opt.MEM_ALIGNMENT == 4).
		 */
		public static int LWIP_MEM_ALIGN_SIZE(int size) { return (((size) + opt.MEM_ALIGNMENT - 1) & ~(opt.MEM_ALIGNMENT - 1)); }

		/** Calculate safe memory size for an aligned buffer when using an unaligned
		 * type as storage. This includes a safety-margin on (opt.MEM_ALIGNMENT - 1) at the
		 * start (e.g. if buffer is byte[] and actual data will be uint)
		 */
		public static int LWIP_MEM_ALIGN_BUFFER(int size) { return (((size) + opt.MEM_ALIGNMENT - 1)); }

		/** Align a memory pointer to the alignment defined by opt.MEM_ALIGNMENT
		 * so that ADDR % opt.MEM_ALIGNMENT == 0
		 */
		public static pointer LWIP_MEM_ALIGN(pointer addr) { return new pointer(addr.data, (addr.offset + opt.MEM_ALIGNMENT - 1) & ~(opt.MEM_ALIGNMENT - 1)); }

#if !MEM_LIBC_MALLOC // don't build if not configured for use in lwipopts.h

#if MEM_USE_POOLS
		/* lwIP head implemented with different sized pools */

		/**
		 * Allocate memory: determine the smallest pool that is big enough
		 * to contain an element of 'size' and get an element from that pool.
		 *
		 * @param size the size in bytes of the memory needed
		 * @return a pointer to the allocated memory or null if the pool is empty
		 */
		public static byte[] mem_malloc(uint size)
		{
			byte[] ret;
			memp_malloc_helper element;
			memp_t poolnr;
			uint required_size = size + lwip.LWIP_MEM_ALIGN_SIZE(memp_malloc_helper.length);

			for (poolnr = MEMP_POOL_FIRST; poolnr <= MEMP_POOL_LAST; poolnr = (memp_t)(poolnr + 1))
			{
#if MEM_USE_POOLS_TRY_BIGGER_POOL
again:
#endif // MEM_USE_POOLS_TRY_BIGGER_POOL
				/* is this pool big enough to hold an element of the required size
				   plus a struct memp_malloc_helper that saves the pool this element came from? */
				if (required_size <= memp_sizes[poolnr])
				{
					break;
				}
			}
			if (poolnr > MEMP_POOL_LAST)
			{
				lwip.LWIP_ASSERT("mem_malloc(): no pool is that big!", false);
				return null;
			}
			element = (memp_malloc_helper)lwip.memp_malloc(poolnr);
			if (element == null)
			{
				/* No need to DEBUGF or ASSERT: This error is already
				   taken care of in memp.c */
#if MEM_USE_POOLS_TRY_BIGGER_POOL
			/** Try a bigger pool if this one is empty! */
			if (poolnr < MEMP_POOL_LAST) {
				poolnr++;
				goto again;
			}
#endif // MEM_USE_POOLS_TRY_BIGGER_POOL
				return null;
			}

			/* save the pool number this element came from */
			element.poolnr = poolnr;
			/* and return a pointer to the memory directly after the struct memp_malloc_helper */
			ret = element + lwip.LWIP_MEM_ALIGN_SIZE(memp_malloc_helper.length);

			return ret;
		}

		/**
		 * Free memory previously allocated by mem_malloc. Loads the pool number
		 * and calls memp_free with that pool number to put the element back into
		 * its pool
		 *
		 * @param rmem the memory element to free
		 */
		void
		mem_free(byte[] rmem)
		{
			memp_malloc_helper hmem;

			lwip.LWIP_ASSERT("rmem != null", (rmem != null));
			lwip.LWIP_ASSERT("rmem == MEM_ALIGN(rmem)", (rmem == lwip.LWIP_MEM_ALIGN(rmem)));

			/* get the original struct memp_malloc_helper */
			hmem = (memp_malloc_helper)(rmem - lwip.LWIP_MEM_ALIGN_SIZE(memp_malloc_helper.length));

			lwip.LWIP_ASSERT("hmem != null", (hmem != null));
			lwip.LWIP_ASSERT("hmem == MEM_ALIGN(hmem)", (hmem == lwip.LWIP_MEM_ALIGN(hmem)));
			lwip.LWIP_ASSERT("hmem.poolnr < memp_t.MEMP_MAX", (hmem.poolnr < memp_t.MEMP_MAX));

			/* and put it in the pool we saved earlier */
			lwip.memp_free(hmem.poolnr, hmem);
		}

#else // MEM_USE_POOLS
	}
	/* lwIP replacement for your libc malloc() */

	/**
	 * The heap is made up as a list of structs of this type.
	 * This does not have to be aligned since for getting its size,
	 * we only use the macro SIZEOF_STRUCT_MEM, which automatically alignes.
	 */
	public partial class mem : pointer
	{
		lwip lwip;

		public new const int length = 12;
		private pointer _next;
		private pointer _prev;
		private pointer _used;

		private mem(lwip lwip, byte[] buffer, int offset)
			: base(buffer, offset)
		{
			this.lwip = lwip;
			_next = new pointer(buffer, offset + 0);
			_prev = new pointer(buffer, offset + 4);
			_used = new pointer(buffer, offset + 8);
		}

		public mem(mem mem, int offset)
			: this(mem.lwip, mem.data, mem.offset + offset)
		{
		}

		public mem(lwip lwip, pointer buffer)
				: this(lwip, buffer.data, buffer.offset)
		{
		}

		/** index (. ram[next]) of the next */
		internal int next { get { return (int)_next; } set { _next.SetValue(value); } }
		/** index (. ram[prev]) of the previous */
		internal int prev { get { return (int)_prev; } set { _prev.SetValue(value); } }
		/** 1: this area is used; 0: this area is unused */
		internal byte used { get { return (byte)_used; } set { _used.SetValue(value); } }

		/** All allocated blocks will be MIN_SIZE bytes big, at least!
		 * MIN_SIZE can be overridden to suit your needs. Smaller values save space,
		 * larger values could prevent too small blocks to fragment the RAM too much. */
#if !MIN_SIZE
		public const int MIN_SIZE = length;
#endif // MIN_SIZE
		/* some alignment macros: we define them here for better source code layout */
		public static readonly int MIN_SIZE_ALIGNED = lwip.LWIP_MEM_ALIGN_SIZE(MIN_SIZE);
		public static readonly int SIZEOF_STRUCT_MEM = lwip.LWIP_MEM_ALIGN_SIZE(mem.length);
		public static readonly int MEM_SIZE_ALIGNED = lwip.LWIP_MEM_ALIGN_SIZE(opt.MEM_SIZE);
	}

	partial class lwip
	{
		/** If you want to relocate the heap to external memory, simply define
		 * LWIP_RAM_HEAP_POINTER as a void-pointer to that location.
		 * If so, make sure the memory at that location is big enough (see below on
		 * how that space is calculated). */
#if !LWIP_RAM_HEAP_POINTER
		/** the heap. we need one struct mem at the end and some room for alignment */
		public pointer ram_heap = new pointer(new byte[mem.MEM_SIZE_ALIGNED + (2 * mem.SIZEOF_STRUCT_MEM) + opt.MEM_ALIGNMENT], 0);
		public pointer LWIP_RAM_HEAP_POINTER { get { return ram_heap; } }
#endif // LWIP_RAM_HEAP_POINTER

		/** pointer to the heap (ram_heap): for alignment, ram is now a pointer instead of an array */
		internal mem ram;
		/** the last entry, always unused! */
		internal mem ram_end;
		/** pointer to the lowest free block, this is used for faster search */
		internal mem lfree;

		/** concurrent access protection */
		//#if !NO_SYS
		internal sys_mutex_t mem_mutex = new sys_mutex_t();
		//#endif
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT

		private volatile byte mem_free_count;

		/* Allow mem_free from other (e.g. interrupt) context */
		public void LWIP_MEM_FREE_DECL_PROTECT() { sys.SYS_ARCH_DECL_PROTECT(lev_free); }
		public void LWIP_MEM_FREE_PROTECT() { sys.SYS_ARCH_PROTECT(lev_free); }
		public void LWIP_MEM_FREE_UNPROTECT() { sys.SYS_ARCH_UNPROTECT(lev_free); }
		public void LWIP_MEM_ALLOC_DECL_PROTECT() { sys.SYS_ARCH_DECL_PROTECT(lev_alloc); }
		public void LWIP_MEM_ALLOC_PROTECT() { sys.SYS_ARCH_PROTECT(lev_alloc); }
		public void LWIP_MEM_ALLOC_UNPROTECT() { sys.SYS_ARCH_UNPROTECT(lev_alloc); }

#else // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT

		/* Protect the heap only by using a semaphore */
		public void LWIP_MEM_FREE_DECL_PROTECT() { }
		public void LWIP_MEM_FREE_PROTECT() { sys.sys_mutex_lock(mem_mutex); }
		public void LWIP_MEM_FREE_UNPROTECT() { sys.sys_mutex_unlock(mem_mutex); }
		/* mem_malloc is protected using semaphore AND LWIP_MEM_ALLOC_PROTECT */
		public void LWIP_MEM_ALLOC_DECL_PROTECT() { }
		public void LWIP_MEM_ALLOC_PROTECT() { }
		public void LWIP_MEM_ALLOC_UNPROTECT() { }

#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT

		/**
		 * "Plug holes" by combining adjacent empty struct mems.
		 * After this function is through, there should not exist
		 * one empty struct mem pointing to another empty struct mem.
		 *
		 * @param mem this points to a struct mem which just has been freed
		 * @internal this function is only called by mem_free() and mem_trim()
		 *
		 * This assumes access to the heap is protected by the calling function
		 * already.
		 */
		private void plug_holes(mem mem)
		{
			mem nmem;
			mem pmem;

			lwip.LWIP_ASSERT("plug_holes: mem >= ram", mem >= ram);
			lwip.LWIP_ASSERT("plug_holes: mem < ram_end", mem < ram_end);
			lwip.LWIP_ASSERT("plug_holes: mem.used == 0", mem.used == 0);

			/* plug hole forward */
			lwip.LWIP_ASSERT("plug_holes: mem.next <= MEM_SIZE_ALIGNED", mem.next <= mem.MEM_SIZE_ALIGNED);

			nmem = new mem(ram, mem.next);
			if (mem != nmem && nmem.used == 0 && nmem != ram_end)
			{
				/* if mem.next is unused and not end of ram, combine mem and mem.next */
				if (lfree == nmem)
				{
					lfree = mem;
				}
				mem.next = nmem.next;
				new mem(ram, nmem.next).prev = mem - ram;
			}

			/* plug hole backward */
			pmem = new mem(ram, mem.prev);
			if (pmem != mem && pmem.used == 0)
			{
				/* if mem.prev is unused, combine mem and mem.prev */
				if (lfree == mem)
				{
					lfree = pmem;
				}
				pmem.next = mem.next;
				new mem(ram, mem.next).prev = pmem - ram;
			}
		}

		/**
		 * Zero the heap and initialize start, end and lowest-free
		 */
		public void mem_init()
		{
			mem mem;

			lwip.LWIP_ASSERT("Sanity check alignment",
			  (mem.SIZEOF_STRUCT_MEM & (opt.MEM_ALIGNMENT - 1)) == 0);

			/* align the heap */
			ram = new mem(this, lwip.LWIP_MEM_ALIGN(LWIP_RAM_HEAP_POINTER));
			/* initialize the start of the heap */
			mem = new mem(ram, 0);
			mem.next = mem.MEM_SIZE_ALIGNED;
			mem.prev = 0;
			mem.used = 0;
			/* initialize the end of the heap */
			ram_end = new mem(ram, mem.MEM_SIZE_ALIGNED);
			ram_end.used = 1;
			ram_end.next = mem.MEM_SIZE_ALIGNED;
			ram_end.prev = mem.MEM_SIZE_ALIGNED;

			/* initialize the lowest-free pointer to the start of the heap */
			lfree = new mem(ram, 0);

			lwip_stats.mem.avail = (uint)mem.MEM_SIZE_ALIGNED;

			if (sys.sys_mutex_new(mem_mutex) != err_t.ERR_OK)
			{
				lwip.LWIP_ASSERT("failed to create mem_mutex", false);
			}
		}

		/**
		 * Put a struct mem back on the heap
		 *
		 * @param rmem is the data portion of a struct mem as returned by a previous
		 *             call to mem_malloc()
		 */
		public void mem_free(pointer rmem)
		{
			mem mem;
			LWIP_MEM_FREE_DECL_PROTECT();

			if (rmem == null)
			{
				lwip.LWIP_DEBUGF(opt.MEM_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_LEVEL_SERIOUS, "mem_free(p == null) was called.\n");
				return;
			}
			lwip.LWIP_ASSERT("mem_free: sanity check alignment", ((rmem.offset) & (opt.MEM_ALIGNMENT - 1)) == 0);

			lwip.LWIP_ASSERT("mem_free: legal memory", rmem >= ram &&
				rmem < ram_end);

			if (rmem < ram || rmem >= ram_end)
			{
				sys.SYS_ARCH_DECL_PROTECT(sys.lev);
				lwip.LWIP_DEBUGF(opt.MEM_DEBUG | lwip.LWIP_DBG_LEVEL_SEVERE, "mem_free: illegal memory\n");
				/* protect mem stats from concurrent access */
				sys.SYS_ARCH_PROTECT(sys.lev);
				++lwip_stats.mem.illegal;
				sys.SYS_ARCH_UNPROTECT(sys.lev);
				return;
			}
			/* protect the heap from concurrent access */
			LWIP_MEM_FREE_PROTECT();
			/* Get the corresponding struct mem ... */
			mem = new mem(this, rmem - mem.SIZEOF_STRUCT_MEM);
			/* ... which has to be in a used state ... */
			lwip.LWIP_ASSERT("mem_free: mem.used", mem.used != 0);
			/* ... and is now unused. */
			mem.used = 0;

			if (mem < lfree)
			{
				/* the newly freed struct is now the lowest */
				lfree = mem;
			}

			lwip_stats.mem.used -= (uint)(mem.next - (mem - ram));

			/* finally, see if prev or next are free also */
			plug_holes(mem);
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			mem_free_count = 1;
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			LWIP_MEM_FREE_UNPROTECT();
		}

		/**
		 * Shrink memory returned by mem_malloc().
		 *
		 * @param rmem pointer to memory allocated by mem_malloc the is to be shrinked
		 * @param newsize required size after shrinking (needs to be smaller than or
		 *                equal to the previous size)
		 * @return for compatibility reasons: is always == rmem, at the moment
		 *         or null if newsize is > old size, in which case rmem is NOT touched
		 *         or freed!
		 */
		public pointer mem_trim(pointer rmem, int newsize)
		{
			int size;
			int ptr, ptr2;
			mem mem, mem2;
			/* use the FREE_PROTECT here: it protects with sem OR sys.SYS_ARCH_PROTECT */
			LWIP_MEM_FREE_DECL_PROTECT();

			/* Expand the size of the allocated memory region so that we can
			   adjust for alignment. */
			newsize = lwip.LWIP_MEM_ALIGN_SIZE(newsize);

			if (newsize < mem.MIN_SIZE_ALIGNED)
			{
				/* every data block must be at least MIN_SIZE_ALIGNED long */
				newsize = mem.MIN_SIZE_ALIGNED;
			}

			if (newsize > mem.MEM_SIZE_ALIGNED)
			{
				return null;
			}

			lwip.LWIP_ASSERT("mem_trim: legal memory", rmem >= ram && rmem < ram_end);

			if (rmem < ram || rmem >= ram_end)
			{
				sys.SYS_ARCH_DECL_PROTECT(sys.lev);
				lwip.LWIP_DEBUGF(opt.MEM_DEBUG | lwip.LWIP_DBG_LEVEL_SEVERE, "mem_trim: illegal memory\n");
				/* protect mem stats from concurrent access */
				sys.SYS_ARCH_PROTECT(sys.lev);
				++lwip_stats.mem.illegal;
				sys.SYS_ARCH_UNPROTECT(sys.lev);
				return rmem;
			}
			/* Get the corresponding mem ... */
			mem = new mem(this, rmem - mem.SIZEOF_STRUCT_MEM);
			/* ... and its offset pointer */
			ptr = mem - ram;

			size = mem.next - ptr - mem.SIZEOF_STRUCT_MEM;
			lwip.LWIP_ASSERT("mem_trim can only shrink memory", newsize <= size);
			if (newsize > size)
			{
				/* not supported */
				return null;
			}
			if (newsize == size)
			{
				/* No change in size, simply return */
				return rmem;
			}

			/* protect the heap from concurrent access */
			LWIP_MEM_FREE_PROTECT();

			mem2 = new mem(ram, mem.next);
			if (mem2.used == 0)
			{
				/* The next is unused, we can simply move it at little */
				int next;
				/* remember the old next pointer */
				next = mem2.next;
				/* create new struct mem which is moved directly after the shrinked mem */
				ptr2 = ptr + mem.SIZEOF_STRUCT_MEM + (int)newsize;
				if (lfree == mem2)
				{
					lfree = new mem(ram, ptr2);
				}
				mem2 = new mem(ram, ptr2);
				mem2.used = 0;
				/* restore the next pointer */
				mem2.next = next;
				/* link it back to mem */
				mem2.prev = ptr;
				/* link mem to it */
				mem.next = ptr2;
				/* last thing to restore linked list: as we have moved mem2,
				 * let 'mem2.next.prev' point to mem2 again. but only if mem2.next is not
				 * the end of the heap */
				if (mem2.next != mem.MEM_SIZE_ALIGNED)
				{
					(new mem(ram, mem2.next)).prev = ptr2;
				}
				lwip_stats.mem.used -= (uint)(size - newsize);
				/* no need to plug holes, we've already done that */
			}
			else if (newsize + mem.SIZEOF_STRUCT_MEM + mem.MIN_SIZE_ALIGNED <= size)
			{
				/* Next struct is used but there's room for another struct mem with
				 * at least MIN_SIZE_ALIGNED of data.
				 * Old size ('size') must be big enough to contain at least 'newsize' plus a struct mem
				 * ('SIZEOF_STRUCT_MEM') with some data ('MIN_SIZE_ALIGNED').
				 * @todo we could leave out MIN_SIZE_ALIGNED. We would create an empty
				 *       region that couldn't hold data, but when mem.next gets freed,
				 *       the 2 regions would be combined, resulting in more free memory */
				ptr2 = ptr + mem.SIZEOF_STRUCT_MEM + (int)newsize;
				mem2 = new mem(ram, ptr2);
				if (mem2 < lfree)
				{
					lfree = mem2;
				}
				mem2.used = 0;
				mem2.next = mem.next;
				mem2.prev = ptr;
				mem.next = ptr2;
				if (mem2.next != mem.MEM_SIZE_ALIGNED)
				{
					(new mem(ram, mem2.next)).prev = ptr2;
				}
				lwip_stats.mem.used -= (uint)(size - newsize);
				/* the original mem.next is used, so no need to plug holes! */
			}
			/* else {
			  next struct mem is used but size between mem and mem2 is not big enough
			  to create another struct mem
			  . don't do anyhting. 
			  . the remaining space stays unused since it is too small
			} */
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			mem_free_count = 1;
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			LWIP_MEM_FREE_UNPROTECT();
			return rmem;
		}

		/**
		 * Adam's mem_malloc() plus solution for bug #17922
		 * Allocate a block of memory with a minimum of 'size' bytes.
		 *
		 * @param size is the minimum size of the requested block in bytes.
		 * @return pointer to allocated memory or null if no free memory was found.
		 *
		 * Note that the returned value will always be aligned (as defined by MEM_ALIGNMENT).
		 */
		public pointer mem_malloc(int size)
		{
			int ptr, ptr2;
			mem mem, mem2;
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			byte local_mem_free_count = 0;
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			LWIP_MEM_ALLOC_DECL_PROTECT();

			if (size == 0)
			{
				return null;
			}

			/* Expand the size of the allocated memory region so that we can
			   adjust for alignment. */
			size = lwip.LWIP_MEM_ALIGN_SIZE(size);

			if (size < mem.MIN_SIZE_ALIGNED)
			{
				/* every data block must be at least MIN_SIZE_ALIGNED long */
				size = mem.MIN_SIZE_ALIGNED;
			}

			if (size > mem.MEM_SIZE_ALIGNED)
			{
				return null;
			}

			/* protect the heap from concurrent access */
			sys.sys_mutex_lock(mem_mutex);
			LWIP_MEM_ALLOC_PROTECT();
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			/* run as long as a mem_free disturbed mem_malloc or mem_trim */
			do
			{
				local_mem_free_count = 0;
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT

			/* Scan through the heap searching for a free block that is big enough,
			 * beginning with the lowest free block.
			 */
			for (ptr = lfree - ram; ptr < mem.MEM_SIZE_ALIGNED - size;
				 ptr = (new mem(ram, ptr)).next)
			{
				mem = new mem(ram, ptr);
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
				mem_free_count = 0;
				LWIP_MEM_ALLOC_UNPROTECT();
				/* allow mem_free or mem_trim to run */
				LWIP_MEM_ALLOC_PROTECT();
				if (mem_free_count != 0)
				{
					/* If mem_free or mem_trim have run, we have to restart since they
						could have altered our current struct mem. */
					local_mem_free_count = 1;
					break;
				}
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT

				if ((mem.used == 0) &&
					(mem.next - (ptr + mem.SIZEOF_STRUCT_MEM)) >= size)
				{
					/* mem is not used and at least perfect fit is possible:
					 * mem.next - (ptr + SIZEOF_STRUCT_MEM) gives us the 'user data size' of mem */

					if (mem.next - (ptr + mem.SIZEOF_STRUCT_MEM) >= (size + mem.SIZEOF_STRUCT_MEM + mem.MIN_SIZE_ALIGNED))
					{
						/* (in addition to the above, we test if another mem (SIZEOF_STRUCT_MEM) containing
						 * at least MIN_SIZE_ALIGNED of data also fits in the 'user data space' of 'mem')
						 * . split large block, create empty remainder,
						 * remainder must be large enough to contain MIN_SIZE_ALIGNED data: if
						 * mem.next - (ptr + (2SIZEOF_STRUCT_MEM)) == size,
						 * struct mem would fit in but no data between mem2 and mem2.next
						 * @todo we could leave out MIN_SIZE_ALIGNED. We would create an empty
						 *       region that couldn't hold data, but when mem.next gets freed,
						 *       the 2 regions would be combined, resulting in more free memory
						 */
						ptr2 = ptr + mem.SIZEOF_STRUCT_MEM + size;
						/* create mem2 struct */
						mem2 = new mem(ram, ptr2);
						mem2.used = 0;
						mem2.next = mem.next;
						mem2.prev = ptr;
						/* and insert it between mem and mem.next */
						mem.next = ptr2;
						mem.used = 1;

						if (mem2.next != mem.MEM_SIZE_ALIGNED)
						{
							(new mem(ram, mem2.next)).prev = ptr2;
						}
						lwip_stats.mem.used += (uint)(size + mem.SIZEOF_STRUCT_MEM);
						if (lwip_stats.mem.max < lwip_stats.mem.used)
						{
							lwip_stats.mem.max = lwip_stats.mem.used;
						}
					}
					else
					{
						/* (a mem2 struct does no fit into the user data space of mem and mem.next will always
						 * be used at this point: if not we have 2 unused structs in a row, plug_holes should have
						 * take care of this).
						 * . near fit or excact fit: do not split, no mem2 creation
						 * also can't move mem.next directly behind mem, since mem.next
						 * will always be used at this point!
						 */
						mem.used = 1;
						lwip_stats.mem.used += (uint)(mem.next - (mem - ram));
						if (lwip_stats.mem.max < lwip_stats.mem.used)
						{
							lwip_stats.mem.max = lwip_stats.mem.used;
						}
					}
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
				mem_malloc_adjust_lfree:
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
					if (mem == lfree)
					{
						mem cur = lfree;
						/* Find next free block after mem and update lowest free pointer */
						while (cur.used != 0 && cur != ram_end)
						{
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
							mem_free_count = 0;
							LWIP_MEM_ALLOC_UNPROTECT();
							/* prevent high interrupt latency... */
							LWIP_MEM_ALLOC_PROTECT();
							if (mem_free_count != 0)
							{
								/* If mem_free or mem_trim have run, we have to restart since they
								   could have altered our current struct mem or lfree. */
								goto mem_malloc_adjust_lfree;
							}
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
							cur = new mem(ram, cur.next);
						}
						lfree = cur;
						lwip.LWIP_ASSERT("mem_malloc: !lfree.used", ((lfree == ram_end) || (lfree.used == 0)));
					}
					LWIP_MEM_ALLOC_UNPROTECT();
					sys.sys_mutex_unlock(mem_mutex);
					lwip.LWIP_ASSERT("mem_malloc: allocated memory not above ram_end.",
						mem + mem.SIZEOF_STRUCT_MEM + size <= ram_end);
					lwip.LWIP_ASSERT("mem_malloc: allocated memory properly aligned.",
						(mem.offset + mem.SIZEOF_STRUCT_MEM) % opt.MEM_ALIGNMENT == 0);
					lwip.LWIP_ASSERT("mem_malloc: sanity check alignment",
						(mem.offset & (opt.MEM_ALIGNMENT - 1)) == 0);

					return mem + mem.SIZEOF_STRUCT_MEM;
				}
			}
#if LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
				/* if we got interrupted by a mem_free, try again */
			} while (local_mem_free_count != 0);
#endif // LWIP_ALLOW_MEM_FREE_FROM_OTHER_CONTEXT
			lwip.LWIP_DEBUGF(opt.MEM_DEBUG | lwip.LWIP_DBG_LEVEL_SERIOUS, "mem_malloc: could not allocate {0} bytes\n", (short)size);
			++lwip_stats.mem.err;
			LWIP_MEM_ALLOC_UNPROTECT();
			sys.sys_mutex_unlock(mem_mutex);
			return null;
		}

#endif // MEM_USE_POOLS
		/**
		 * Contiguously allocates enough space for count objects that are size bytes
		 * of memory each and returns a pointer to the allocated memory.
		 *
		 * The allocated memory is filled with bytes of value zero.
		 *
		 * @param count number of objects to allocate
		 * @param size size of the objects to allocate
		 * @return pointer to allocated memory / null pointer if there is an error
		 */
		public pointer mem_calloc(int count, int size)
		{
			pointer p;

			/* allocate 'count' objects of size 'size' */
			p = mem_malloc(count * size);
			if (p != null)
			{
				/* zero the memory */
				mem.memset(p, 0, count * size);
			}
			return p;
		}

#endif // !MEM_LIBC_MALLOC
	}
}
