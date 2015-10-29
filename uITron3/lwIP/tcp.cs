/**
 * @file
 * Transmission Control Protocol for IP
 *
 * This file contains common functions for the TCP implementation, such as functinos
 * for manipulating the data structures and the TCP timer functions. TCP functions
 * related to input and output is found in tcp_in.c and tcp_out.c respectively.
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
	/** Function prototype for tcp accept callback functions. Called when a new
	 * connection can be accepted on a listening pcb.
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param newpcb The new connection pcb
	 * @param err An error code if there has been an error accepting.
	 *            Only return err_t.ERR_ABRT if you have called tcp.tcp_abort from within the
	 *            callback function!
	 */
	public delegate err_t tcp_accept_fn(object arg, tcp_pcb newpcb, err_t err);

	/** Function prototype for tcp receive callback functions. Called when data has
	 * been received.
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param tpcb The connection pcb which received data
	 * @param p The received data (or null when the connection has been closed!)
	 * @param err An error code if there has been an error receiving
	 *            Only return err_t.ERR_ABRT if you have called tcp.tcp_abort from within the
	 *            callback function!
	 */
	public delegate err_t tcp_recv_fn(object arg, tcp_pcb tpcb, pbuf p, err_t err);

	/** Function prototype for tcp sent callback functions. Called when sent data has
	 * been acknowledged by the remote side. Use it to free corresponding resources.
	 * This also means that the pcb has now space available to send new data.
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param tpcb The connection pcb for which data has been acknowledged
	 * @param len The amount of bytes acknowledged
	 * @return err_t.ERR_OK: try to send some data by calling tcp.tcp_output
	 *            Only return err_t.ERR_ABRT if you have called tcp.tcp_abort from within the
	 *            callback function!
	 */
	public delegate err_t tcp_sent_fn(object arg, tcp_pcb tpcb, ushort len);

	/** Function prototype for tcp poll callback functions. Called periodically as
	 * specified by @see tcp_poll.
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param tpcb tcp pcb
	 * @return err_t.ERR_OK: try to send some data by calling tcp.tcp_output
	 *            Only return err_t.ERR_ABRT if you have called tcp.tcp_abort from within the
	 *            callback function!
	 */
	public delegate err_t tcp_poll_fn(object arg, tcp_pcb tpcb);

	/** Function prototype for tcp error callback functions. Called when the pcb
	 * receives a RST or is unexpectedly closed for any other reason.
	 *
	 * @note The corresponding pcb is already freed when this callback is called!
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param err Error code to indicate why the pcb has been closed
	 *            err_t.ERR_ABRT: aborted through tcp.tcp_abort or by a TCP timer
	 *            err_t.ERR_RST: the connection was reset by the remote host
	 */
	public delegate void tcp_err_fn(object arg, err_t err);

	/** Function prototype for tcp connected callback functions. Called when a pcb
	 * is connected to the remote side after initiating a connection attempt by
	 * calling tcp_connect().
	 *
	 * @param arg Additional argument to pass to the callback function (@see tcp_arg())
	 * @param tpcb The connection pcb which is connected
	 * @param err An unused error code, always err_t.ERR_OK currently ;-) TODO!
	 *            Only return err_t.ERR_ABRT if you have called tcp.tcp_abort from within the
	 *            callback function!
	 *
	 * @note When a connection attempt fails, the error callback is currently called!
	 */
	public delegate err_t tcp_connected_fn(object arg, tcp_pcb tpcb, err_t err);

	public enum tcp_state
	{
		CLOSED = 0,
		LISTEN = 1,
		SYN_SENT = 2,
		SYN_RCVD = 3,
		ESTABLISHED = 4,
		FIN_WAIT_1 = 5,
		FIN_WAIT_2 = 6,
		CLOSE_WAIT = 7,
		CLOSING = 8,
		LAST_ACK = 9,
		TIME_WAIT = 10
	};

	/**
	 * members common to tcp_pcb and tcp_listen_pcb
	 */
	public class tcp_pcb_common : ip_pcb
	{
		public tcp_pcb_common(lwip lwip)
			: base(lwip)
		{
		}

		public tcp_pcb_common next; /* for the linked list */
		public object callback_arg;
		/* the accept callback for listen- and normal pcbs, if LWIP_CALLBACK_API */
		public tcp_accept_fn accept;
		public tcp_state state; /* TCP state */
		public byte prio;
		/* ports are in host byte order */
		public ushort local_port;
	}

	/* the TCP protocol control block */
	public class tcp_pcb : tcp_pcb_common
	{
		public tcp_pcb(lwip lwip)
			: base(lwip)
		{
		}

		/* ports are in host byte order */
		public ushort remote_port;

		public byte flags;
		public const byte TF_ACK_DELAY = (byte)0x01U;   /* Delayed ACK. */
		public const byte TF_ACK_NOW = (byte)0x02U;   /* Immediate ACK. */
		public const byte TF_INFR = (byte)0x04U;   /* In fast recovery. */
		public const byte TF_TIMESTAMP = (byte)0x08U;   /* Timestamp option enabled */
		public const byte TF_RXCLOSED = (byte)0x10U;   /* rx closed by tcp_shutdown */
		public const byte TF_FIN = (byte)0x20U;   /* Connection was closed locally (FIN segment enqueued). */
		public const byte TF_NODELAY = (byte)0x40U;   /* Disable Nagle algorithm */
		public const byte TF_NAGLEMEMERR = (byte)0x80U;   /* nagle enabled, memerr, try to output to prevent delayed ACK to happen */

		/* the rest of the fields are in host byte order
		   as we have to do some math with them */

		/* Timers */
		public byte polltmr, pollinterval;
		public byte last_timer;
		public uint tmr;

		/* receiver variables */
		public uint rcv_nxt;   /* next seqno expected */
		public ushort rcv_wnd;   /* receiver window available */
		public ushort rcv_ann_wnd; /* receiver window to announce */
		public uint rcv_ann_right_edge; /* announced right edge of window */

		/* Retransmission timer. */
		public short rtime;

		public ushort mss;   /* maximum segment size */

		/* RTT (round trip time) estimation variables */
		public uint rttest; /* RTT estimate in 500ms ticks */
		public uint rtseq;  /* sequence number being timed */
		public short sa, sv; /* @todo document this */

		public short rto;    /* retransmission time-out */
		public byte nrtx;    /* number of retransmissions */

		/* fast retransmit/recovery */
		public byte dupacks;
		public uint lastack; /* Highest acknowledged seqno. */

		/* congestion avoidance/control variables */
		public ushort cwnd;
		public ushort ssthresh;

		/* sender variables */
		public uint snd_nxt;            /* next new seqno to be sent */
		public uint snd_wl1, snd_wl2;   /* Sequence and acknowledgement numbers of last
										   window update. */
		public uint snd_lbb;            /* Sequence number of next byte to be buffered. */
		public ushort snd_wnd;          /* sender window */
		public ushort snd_wnd_max;      /* the maximum sender window announced by the remote host */

		public ushort acked;

		public ushort snd_buf;          /* Available buffer space for sending (in bytes). */
		public const uint TCP_SNDQUEUELEN_OVERFLOW = (0xffffU - 3);
		public ushort snd_queuelen;     /* Available buffer space for sending (in tcp_segs). */

#if TCP_OVERSIZE
		/* Extra bytes available at the end of the last pbuf in unsent. */
		public ushort unsent_oversize;
#endif // TCP_OVERSIZE

		/* These are ordered by sequence number: */
		public tcp_seg unsent;   /* Unsent (queued) segments. */
		public tcp_seg unacked;  /* Sent but unacknowledged segments. */
#if TCP_QUEUE_OOSEQ
		public tcp_seg ooseq;    /* Received out of sequence segments. */
#endif // TCP_QUEUE_OOSEQ

		public pbuf refused_data; /* Data previously received but not yet taken by upper layer */

#if LWIP_CALLBACK_API
		/* Function to be called when more send buffer space is available. */
		public tcp_sent_fn sent;
		/* Function to be called when (in-sequence) data has arrived. */
		public tcp_recv_fn recv;
		/* Function to be called when a connection has been set up. */
		public tcp_connected_fn connected;
		/* Function which is called periodically. */
		public tcp_poll_fn poll;
		/* Function to be called whenever a fatal error occurs. */
		public tcp_err_fn errf;
#endif // LWIP_CALLBACK_API

#if LWIP_TCP_TIMESTAMPS
		public uint ts_lastacksent;
		public uint ts_recent;
#endif // LWIP_TCP_TIMESTAMPS

		/* idle time before KEEPALIVE is sent */
		public uint keep_idle;
#if LWIP_TCP_KEEPALIVE
		public uint keep_intvl;
		public uint keep_cnt;
#endif // LWIP_TCP_KEEPALIVE

		/* Persist timer counter */
		public byte persist_cnt;
		/* Persist timer back-off */
		public byte persist_backoff;

		/* KEEPALIVE counter */
		public byte keep_cnt_sent;
	};

	public class tcp_pcb_listen : tcp_pcb_common
	{
		public tcp_pcb_listen(lwip lwip)
			: base(lwip)
		{
		}

#if TCP_LISTEN_BACKLOG
		public byte backlog;
		public byte accepts_pending;
#endif // TCP_LISTEN_BACKLOG
	};

#if LWIP_EVENT_API

	public enum lwip_event
	{
		LWIP_EVENT_ACCEPT,
		LWIP_EVENT_SENT,
		LWIP_EVENT_RECV,
		LWIP_EVENT_CONNECTED,
		LWIP_EVENT_POLL,
		LWIP_EVENT_ERR
	};

	public delegate err_t lwip_tcp_event_t(object arg, tcp_pcb pcb, lwip_event e, pbuf p, ushort size, err_t err);

#endif // LWIP_EVENT_API

	public partial class tcp
	{
		lwip lwip;

		public tcp(lwip lwip)
		{
			this.lwip = lwip;
			inseg = new tcp_seg(lwip);
			tcp_pcb_lists = new tcp_pcb_common[]{ tcp_listen_pcbs.pcbs, tcp_bound_pcbs,
				tcp_active_pcbs, tcp_tw_pcbs};
		}

#if LWIP_EVENT_API
		public lwip_tcp_event_t _lwip_tcp_event;
		public err_t lwip_tcp_event(object arg, tcp_pcb pcb, lwip_event e, pbuf p, ushort size, err_t err)
		{
			return _lwip_tcp_event(arg, pcb, e, p, size, err);
		}
#endif
		public static ushort tcp_mss(tcp_pcb pcb) { return (ushort)((pcb.flags & tcp_pcb.TF_TIMESTAMP) != 0 ? (pcb.mss - 12) : pcb.mss); }
		public static ushort tcp_sndbuf(tcp_pcb pcb) { return pcb.snd_buf; }
		public static ushort tcp_sndqueuelen(tcp_pcb pcb) { return pcb.snd_queuelen; }
		public static ushort tcp_nagle_disable(tcp_pcb pcb) { return pcb.flags |= tcp_pcb.TF_NODELAY; }
		public static ushort tcp_nagle_enable(tcp_pcb pcb) { return pcb.flags &= unchecked((byte)~tcp_pcb.TF_NODELAY); }
		public static bool tcp_nagle_disabled(tcp_pcb pcb) { return (pcb.flags & tcp_pcb.TF_NODELAY) != 0; }

#if TCP_LISTEN_BACKLOG
		public static void tcp_accepted(tcp_pcb_common pcb)
		{
			do
			{
				lwip.LWIP_ASSERT("pcb.state == tcp_state.LISTEN (called for wrong pcb?)", pcb.state == tcp_state.LISTEN);
				((tcp_pcb_listen)(pcb)).accepts_pending--;
			} while (false);
		}
#else  // TCP_LISTEN_BACKLOG
		public static void tcp_accepted(tcp_pcb pcb)
		{
			lwip.LWIP_ASSERT("pcb.state == tcp_state.LISTEN (called for wrong pcb?)", (pcb).state == tcp_state.LISTEN);
		}
#endif // TCP_LISTEN_BACKLOG

		public tcp_pcb_listen tcp_listen(tcp_pcb_common pcb) { return tcp_listen_with_backlog(pcb, opt.TCP_DEFAULT_LISTEN_BACKLOG); }

		/* Flags for "apiflags" parameter in tcp_write */
		public const int TCP_WRITE_FLAG_COPY = 0x01;
		public const int TCP_WRITE_FLAG_MORE = 0x02;

		public const int TCP_PRIO_MIN = 1;
		public const int TCP_PRIO_NORMAL = 64;
		public const int TCP_PRIO_MAX = 127;
	}


	public partial class tcp
	{
		/* From http://www.iana.org/assignments/port-numbers:
		   "The Dynamic and/or Private Ports are those from 49152 through 65535" */
		public const ushort TCP_LOCAL_PORT_RANGE_START = 0xc000;
		public const ushort TCP_LOCAL_PORT_RANGE_END = 0xffff;
		public static ushort TCP_ENSURE_LOCAL_PORT_RANGE(ushort port) { return (ushort)(((port) & ~TCP_LOCAL_PORT_RANGE_START) + TCP_LOCAL_PORT_RANGE_START); }

#if LWIP_TCP_KEEPALIVE
		public static uint TCP_KEEP_DUR(tcp_pcb pcb) { return ((pcb).keep_cnt * (pcb).keep_intvl); }
		public static uint TCP_KEEP_INTVL(tcp_pcb pcb) { return ((pcb).keep_intvl); }
#else // LWIP_TCP_KEEPALIVE
		public static uint TCP_KEEP_DUR(tcp_pcb pcb) { return tcp.TCP_MAXIDLE; }
		public static uint TCP_KEEP_INTVL(tcp_pcb pcb) { return tcp.TCP_KEEPINTVL_DEFAULT; }
#endif // LWIP_TCP_KEEPALIVE

		public static readonly string[] tcp_state_str = new string[]{
			"tcp_state.CLOSED",
			"tcp_state.LISTEN",
			"tcp_state.SYN_SENT",
			"tcp_state.SYN_RCVD",
			"tcp_state.ESTABLISHED",
			"tcp_state.FIN_WAIT_1",
			"tcp_state.FIN_WAIT_2",
			"tcp_state.CLOSE_WAIT",
			"tcp_state.CLOSING",
			"tcp_state.LAST_ACK",
			"tcp_state.TIME_WAIT"
		};

		/* last local TCP port */
		public ushort tcp_port = TCP_LOCAL_PORT_RANGE_START;

		/* Incremented every coarse grained timer shot (typically every 500 ms). */
		public uint tcp_ticks;
		public static readonly byte[] tcp_backoff = new byte[] { 1, 2, 3, 4, 5, 6, 7, 7, 7, 7, 7, 7, 7 };
		/* Times per slowtmr hits */
		public static readonly byte[] tcp_persist_backoff = new byte[] { 3, 6, 12, 24, 48, 96, 120 };

		/* The TCP PCB lists. */

		/** List of all TCP PCBs bound but not yet (connected || listening) */
		public tcp_pcb_common tcp_bound_pcbs;
		/** List of all TCP PCBs in tcp_state.LISTEN state */
		public tcp_listen_pcbs_t tcp_listen_pcbs = new tcp_listen_pcbs_t();
		/** List of all TCP PCBs that are in a state in which
		 * they accept or send data. */
		public tcp_pcb tcp_active_pcbs;
		/** List of all TCP PCBs in TIME-WAIT state */
		public tcp_pcb tcp_tw_pcbs;

		public const int NUM_TCP_PCB_LISTS = 4;
		public const int NUM_TCP_PCB_LISTS_NO_TIME_WAIT = 3;
		/** An array with all (non-temporary) PCB lists, mainly used for smaller code size */
		public tcp_pcb_common[] tcp_pcb_lists;

		/** Only used for temporary storage. */
		public tcp_pcb_common tcp_tmp_pcb;

		public byte tcp_active_pcbs_changed;

		/** Timer counter to handle calling slow-timer from tcp_tmr() */
		private byte tcp_timer;
		private byte tcp_timer_ctr;

		/**
		 * Initialize this module.
		 */
		public static void tcp_init(lwip lwip)
		{
			lwip.tcp = new tcp(lwip);
#if LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS && LWIP_RAND
			lwip.tcp.tcp_port = TCP_ENSURE_LOCAL_PORT_RANGE((ushort)sys.LWIP_RAND());
#endif // LWIP_RANDOMIZE_INITIAL_LOCAL_PORTS && LWIP_RAND
		}

		/**
		 * Called periodically to dispatch TCP timers.
		 */
		public void tcp_tmr()
		{
			/* Call tcp_fasttmr() every 250 ms */
			tcp_fasttmr();

			if ((++tcp_timer & 1) != 0)
			{
				/* Call tcp_tmr() every 500 ms, i.e., every other timer
				   tcp_tmr() is called. */
				tcp_slowtmr();
			}
		}

		/**
		 * Closes the TX side of a connection held by the PCB.
		 * For tcp_close(), a RST is sent if the application didn't receive all data
		 * (tcp_recved() not called for all data passed to recv callback).
		 *
		 * Listening pcbs are freed and may not be referenced any more.
		 * Connection pcbs are freed if not yet connected and may not be referenced
		 * any more. If a connection is established (at least SYN received or in
		 * a closing state), the connection is closed, and put in a closing state.
		 * The pcb is then automatically freed in tcp_slowtmr(). It is therefore
		 * unsafe to reference it.
		 *
		 * @param pcb the tcp_pcb to close
		 * @return err_t.ERR_OK if connection has been closed
		 *         another err_t if closing failed and pcb is not freed
		 */
		private err_t tcp_close_shutdown(tcp_pcb_common pcb, byte rst_on_unacked_data)
		{
			err_t err;

			if (rst_on_unacked_data != 0 && ((pcb.state == tcp_state.ESTABLISHED) || (pcb.state == tcp_state.CLOSE_WAIT)))
			{
				tcp_pcb _pcb = (tcp_pcb)pcb;
				if ((_pcb.refused_data != null) || (_pcb.rcv_wnd != opt.TCP_WND))
				{
					/* Not all data received by application, send RST to tell the remote
					   side about this. */
					lwip.LWIP_ASSERT("pcb.flags & tcp_pcb.TF_RXCLOSED", (_pcb.flags & tcp_pcb.TF_RXCLOSED) != 0);

					/* don't call tcp.tcp_abort here: we must not deallocate the pcb since
					   that might not be expected when calling tcp_close */
					tcp_rst(_pcb.snd_nxt, _pcb.rcv_nxt, _pcb.local_ip, _pcb.remote_ip,
					  _pcb.local_port, _pcb.remote_port);

					tcp_pcb_purge(_pcb);
					TCP_RMV_ACTIVE(_pcb);
					if (_pcb.state == tcp_state.ESTABLISHED)
					{
						/* move to tcp_state.TIME_WAIT since we close actively */
						_pcb.state = tcp_state.TIME_WAIT;
						tcp_pcb_common temp = tcp_tw_pcbs;
						TCP_REG(ref temp, _pcb);
						tcp_tw_pcbs = (tcp_pcb)temp;
					}
					else
					{
						/* tcp_state.CLOSE_WAIT: deallocate the pcb since we already sent a RST for it */
						lwip.memp_free(memp_t.MEMP_TCP_PCB, _pcb);
					}
					return err_t.ERR_OK;
				}
			}

			switch (pcb.state)
			{
				case tcp_state.CLOSED:
					/* Closing a pcb in the tcp_state.CLOSED state might seem erroneous,
					 * however, it is in this state once allocated and as yet unused
					 * and the user needs some way to free it should the need arise.
					 * Calling tcp_close() with a pcb that has already been closed, (i.e. twice)
					 * or for a pcb that has been used and then entered the tcp_state.CLOSED state 
					 * is erroneous, but this should never happen as the pcb has in those cases
					 * been freed, and so any remaining handles are bogus. */
					err = err_t.ERR_OK;
					if (pcb.local_port != 0)
					{
						TCP_RMV(ref tcp_bound_pcbs, pcb);
					}
					lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb);
					pcb = null;
					break;
				case tcp_state.LISTEN:
					err = err_t.ERR_OK;
					tcp_pcb_remove(tcp_listen_pcbs.pcbs, pcb);
					lwip.memp_free(memp_t.MEMP_TCP_PCB_LISTEN, pcb);
					pcb = null;
					break;
				case tcp_state.SYN_SENT:
					err = err_t.ERR_OK;
					TCP_PCB_REMOVE_ACTIVE(pcb);
					lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb);
					pcb = null;
					//snmp.snmp_inc_tcpattemptfails();
					break;
				case tcp_state.SYN_RCVD:
					err = tcp_send_fin((tcp_pcb)pcb);
					if (err == err_t.ERR_OK)
					{
						//snmp.snmp_inc_tcpattemptfails();
						pcb.state = tcp_state.FIN_WAIT_1;
					}
					break;
				case tcp_state.ESTABLISHED:
					err = tcp_send_fin((tcp_pcb)pcb);
					if (err == err_t.ERR_OK)
					{
						//snmp.snmp_inc_tcpestabresets();
						pcb.state = tcp_state.FIN_WAIT_1;
					}
					break;
				case tcp_state.CLOSE_WAIT:
					err = tcp_send_fin((tcp_pcb)pcb);
					if (err == err_t.ERR_OK)
					{
						//snmp.snmp_inc_tcpestabresets();
						pcb.state = tcp_state.LAST_ACK;
					}
					break;
				default:
					/* Has already been closed, do nothing. */
					err = err_t.ERR_OK;
					pcb = null;
					break;
			}

			if (pcb != null && err == err_t.ERR_OK)
			{
				/* To ensure all data has been sent when tcp_close returns, we have
				   to make sure tcp.tcp_output doesn't fail.
				   Since we don't really have to ensure all data has been sent when tcp_close
				   returns (unsent data is sent from tcp timer functions, also), we don't care
				   for the return value of tcp.tcp_output for now. */
				/* @todo: When implementing SO_LINGER, this must be changed somehow:
				   If sof.SOF_LINGER is set, the data should be sent and acked before close returns.
				   This can only be valid for sequential APIs, not for the raw API. */
				tcp_output((tcp_pcb)pcb);
			}
			return err;
		}

		/**
		 * Closes the connection held by the PCB.
		 *
		 * Listening pcbs are freed and may not be referenced any more.
		 * Connection pcbs are freed if not yet connected and may not be referenced
		 * any more. If a connection is established (at least SYN received or in
		 * a closing state), the connection is closed, and put in a closing state.
		 * The pcb is then automatically freed in tcp_slowtmr(). It is therefore
		 * unsafe to reference it (unless an error is returned).
		 *
		 * @param pcb the tcp_pcb to close
		 * @return err_t.ERR_OK if connection has been closed
		 *         another err_t if closing failed and pcb is not freed
		 */
		public err_t tcp_close(tcp_pcb_common pcb)
		{
#if TCP_DEBUG
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_close: closing in ");
			tcp_debug_print_state(pcb.state);
#endif // TCP_DEBUG

			if (pcb.state != tcp_state.LISTEN)
			{
				/* Set a flag not to receive any more data... */
				((tcp_pcb)pcb).flags |= tcp_pcb.TF_RXCLOSED;
			}
			/* ... and close */
			return tcp_close_shutdown(pcb, 1);
		}

		/**
		 * Causes all or part of a full-duplex connection of this PCB to be shut down.
		 * This doesn't deallocate the PCB unless shutting down both sides!
		 * Shutting down both sides is the same as calling tcp_close, so if it succeds,
		 * the PCB should not be referenced any more.
		 *
		 * @param pcb PCB to shutdown
		 * @param shut_rx shut down receive side if this is != 0
		 * @param shut_tx shut down send side if this is != 0
		 * @return err_t.ERR_OK if shutdown succeeded (or the PCB has already been shut down)
		 *         another err_t on error.
		 */
		public err_t tcp_shutdown(tcp_pcb pcb, int shut_rx, int shut_tx)
		{
			if (pcb.state == tcp_state.LISTEN)
			{
				return err_t.ERR_CONN;
			}
			if (shut_rx != 0)
			{
				/* shut down the receive side: set a flag not to receive any more data... */
				pcb.flags |= tcp_pcb.TF_RXCLOSED;
				if (shut_tx != 0)
				{
					/* shutting down the tx AND rx side is the same as closing for the raw API */
					return tcp_close_shutdown(pcb, 1);
				}
				/* ... and free buffered data */
				if (pcb.refused_data != null)
				{
					lwip.pbuf_free(pcb.refused_data);
					pcb.refused_data = null;
				}
			}
			if (shut_tx != 0)
			{
				/* This can't happen twice since if it succeeds, the pcb's state is changed.
				   Only close in these states as the others directly deallocate the PCB */
				switch (pcb.state)
				{
					case tcp_state.SYN_RCVD:
					case tcp_state.ESTABLISHED:
					case tcp_state.CLOSE_WAIT:
						return tcp_close_shutdown(pcb, (byte)shut_rx);
					default:
						/* Not (yet?) connected, cannot shutdown the TX side as that would bring us
						  into tcp_state.CLOSED state, where the PCB is deallocated. */
						return err_t.ERR_CONN;
				}
			}
			return err_t.ERR_OK;
		}

		/**
		 * Abandons a connection and optionally sends a RST to the remote
		 * host.  Deletes the local protocol control block. This is done when
		 * a connection is killed because of shortage of memory.
		 *
		 * @param pcb the tcp_pcb to abort
		 * @param reset boolean to indicate whether a reset should be sent
		 */
		public void tcp_abandon(tcp_pcb pcb, int reset)
		{
			uint seqno, ackno;
#if LWIP_CALLBACK_API
			tcp_err_fn errf;
#endif // LWIP_CALLBACK_API
			object errf_arg;

			/* pcb.state tcp_state.LISTEN not allowed here */
			lwip.LWIP_ASSERT("don't call tcp.tcp_abort/tcp_abandon for listen-pcbs",
			  pcb.state != tcp_state.LISTEN);
			/* Figure out on which TCP PCB list we are, and remove us. If we
			   are in an active state, call the receive function associated with
			   the PCB with a null argument, and send an RST to the remote end. */
			if (pcb.state == tcp_state.TIME_WAIT)
			{
				tcp_pcb_remove(tcp_tw_pcbs, pcb);
				lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb);
			}
			else
			{
				seqno = pcb.snd_nxt;
				ackno = pcb.rcv_nxt;
#if LWIP_CALLBACK_API
				errf = pcb.errf;
#endif // LWIP_CALLBACK_API
				errf_arg = pcb.callback_arg;
				TCP_PCB_REMOVE_ACTIVE(pcb);
				if (pcb.unacked != null)
				{
					tcp_segs_free(pcb.unacked);
				}
				if (pcb.unsent != null)
				{
					tcp_segs_free(pcb.unsent);
				}
#if TCP_QUEUE_OOSEQ
				if (pcb.ooseq != null)
				{
					tcp_segs_free(pcb.ooseq);
				}
#endif // TCP_QUEUE_OOSEQ
				if (reset != 0)
				{
					lwip.LWIP_DEBUGF(opt.TCP_RST_DEBUG, "tcp_abandon: sending RST\n");
					tcp_rst(seqno, ackno, pcb.local_ip, pcb.remote_ip, pcb.local_port, pcb.remote_port);
				}
				lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb);
				TCP_EVENT_ERR(errf, errf_arg, err_t.ERR_ABRT);
			}
		}

		/**
		 * Aborts the connection by sending a RST (reset) segment to the remote
		 * host. The pcb is deallocated. This function never fails.
		 *
		 * ATTENTION: When calling this from one of the TCP callbacks, make
		 * sure you always return err_t.ERR_ABRT (and never return err_t.ERR_ABRT otherwise
		 * or you will risk accessing deallocated memory or memory leaks!
		 *
		 * @param pcb the tcp pcb to abort
		 */
		public void tcp_abort(tcp_pcb pcb)
		{
			tcp_abandon(pcb, 1);
		}

		/**
		 * Binds the connection to a local portnumber and IP address. If the
		 * IP address is not given (i.e., ipaddr == null), the IP address of
		 * the outgoing network interface is used instead.
		 *
		 * @param pcb the tcp_pcb to bind (no check is done whether this pcb is
		 *        already bound!)
		 * @param ipaddr the local ip address to bind to (use IP_ADDR_ANY to bind
		 *        to any local address
		 * @param port the local port to bind to
		 * @return err_t.ERR_USE if the port is already in use
		 *         err_t.ERR_VAL if bind failed because the PCB is not in a valid state
		 *         err_t.ERR_OK if bound
		 */
		public err_t tcp_bind(tcp_pcb pcb, ip_addr ipaddr, ushort port)
		{
			int i;
			int max_pcb_list = NUM_TCP_PCB_LISTS;
			tcp_pcb_common cpcb;

			if (lwip.LWIP_ERROR("tcp_bind: can only bind in state tcp_state.CLOSED", pcb.state == tcp_state.CLOSED)) return err_t.ERR_VAL;

#if SO_REUSE
			/* Unless the REUSEADDR flag is set,
			   we have to check the pcbs in TIME-WAIT state, also.
			   We do not dump tcp_state.TIME_WAIT pcb's; they can still be matched by incoming
			   packets using both local and remote IP addresses and ports to distinguish.
			 */
			if (ip.ip_get_option(pcb, sof.SOF_REUSEADDR))
			{
				max_pcb_list = NUM_TCP_PCB_LISTS_NO_TIME_WAIT;
			}
#endif // SO_REUSE

			if (port == 0)
			{
				port = tcp_new_port();
				if (port == 0)
				{
					return err_t.ERR_BUF;
				}
			}

			/* Check if the address already is in use (on all lists) */
			for (i = 0; i < max_pcb_list; i++)
			{
				for (cpcb = tcp_pcb_lists[i]; cpcb != null; cpcb = cpcb.next)
				{
					if (cpcb.local_port == port)
					{
#if SO_REUSE
						/* Omit checking for the same port if both pcbs have REUSEADDR set.
						   For SO_REUSEADDR, the duplicate-check for a 5-tuple is done in
						   tcp_connect. */
						if (!ip.ip_get_option(pcb, sof.SOF_REUSEADDR) ||
							!ip.ip_get_option(cpcb, sof.SOF_REUSEADDR))
#endif // SO_REUSE
						{
							if (ip_addr.ip_addr_isany(cpcb.local_ip) ||
								ip_addr.ip_addr_isany(ipaddr) ||
								ip_addr.ip_addr_cmp(cpcb.local_ip, ipaddr))
							{
								return err_t.ERR_USE;
							}
						}
					}
				}
			}

			if (!ip_addr.ip_addr_isany(ipaddr))
			{
				ip_addr.ip_addr_copy(pcb.local_ip, ipaddr);
			}
			pcb.local_port = port;
			TCP_REG(ref tcp_bound_pcbs, pcb);
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_bind: bind to port {0}\n", port);
			return err_t.ERR_OK;
		}
#if LWIP_CALLBACK_API
		/**
 * Default accept callback if no accept callback is specified by the user.
 */
		private static err_t tcp_accept_null(object arg, tcp_pcb pcb, err_t err)
		{
			//LWIP_UNUSED_ARG(arg);
			//LWIP_UNUSED_ARG(pcb);
			//LWIP_UNUSED_ARG(err);

			return err_t.ERR_ABRT;
		}
#endif // LWIP_CALLBACK_API

		/**
		 * Set the state of the connection to be tcp_state.LISTEN, which means that it
		 * is able to accept incoming connections. The protocol control block
		 * is reallocated in order to consume less memory. Setting the
		 * connection to tcp_state.LISTEN is an irreversible process.
		 *
		 * @param pcb the original tcp_pcb
		 * @param backlog the incoming connections queue limit
		 * @return tcp_pcb used for listening, consumes less memory.
		 *
		 * @note The original tcp_pcb is freed. This function therefore has to be
		 *       called like this:
		 *             tpcb = tcp_listen(tpcb);
		 */
		public tcp_pcb_listen tcp_listen_with_backlog(tcp_pcb_common pcb, byte backlog)
		{
			tcp_pcb_listen lpcb;

			//LWIP_UNUSED_ARG(backlog);
			if (lwip.LWIP_ERROR("tcp_listen: pcb already connected", pcb.state == tcp_state.CLOSED)) return null;

			/* already listening? */
			if (pcb.state == tcp_state.LISTEN)
			{
				return (tcp_pcb_listen)pcb;
			}
#if SO_REUSE
			if (ip.ip_get_option(pcb, sof.SOF_REUSEADDR))
			{
				/* Since sof.SOF_REUSEADDR allows reusing a local address before the pcb's usage
				   is declared (listen-/connection-pcb), we have to make sure now that
				   this port is only used once for every local IP. */
				for (lpcb = tcp_listen_pcbs.listen_pcbs; lpcb != null; lpcb = (tcp_pcb_listen)lpcb.next)
				{
					if (lpcb.local_port == pcb.local_port)
					{
						if (ip_addr.ip_addr_cmp(lpcb.local_ip, pcb.local_ip))
						{
							/* this address/port is already used */
							return null;
						}
					}
				}
			}
#endif // SO_REUSE
			lpcb = (tcp_pcb_listen)lwip.memp_malloc(memp_t.MEMP_TCP_PCB_LISTEN);
			if (lpcb == null)
			{
				return null;
			}
			lpcb.callback_arg = pcb.callback_arg;
			lpcb.local_port = pcb.local_port;
			lpcb.state = tcp_state.LISTEN;
			lpcb.prio = pcb.prio;
			lpcb.so_options = pcb.so_options;
			ip.ip_set_option(lpcb, (byte)sof.SOF_ACCEPTCONN);
			lpcb.ttl = pcb.ttl;
			lpcb.tos = pcb.tos;
			ip_addr.ip_addr_copy(lpcb.local_ip, pcb.local_ip);
			if (pcb.local_port != 0)
			{
				TCP_RMV(ref tcp_bound_pcbs, pcb);
			}
			lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb);
#if LWIP_CALLBACK_API
			lpcb.accept = tcp_accept_null;
#endif // LWIP_CALLBACK_API
#if TCP_LISTEN_BACKLOG
			lpcb.accepts_pending = 0;
			lpcb.backlog = (backlog != 0) ? backlog : (byte)1;
#endif // TCP_LISTEN_BACKLOG
			tcp_pcb_common temp = tcp_listen_pcbs.pcbs;
			TCP_REG(ref temp, lpcb);
			tcp_listen_pcbs.pcbs = temp;
			return lpcb;
		}

		/** 
		 * Update the state that tracks the available window space to advertise.
		 *
		 * Returns how much extra window would be advertised if we sent an
		 * update now.
		 */
		public static uint tcp_update_rcv_ann_wnd(tcp_pcb pcb)
		{
			uint new_right_edge = pcb.rcv_nxt + pcb.rcv_wnd;

			if (tcp.TCP_SEQ_GEQ(new_right_edge, pcb.rcv_ann_right_edge + Math.Min((uint)(opt.TCP_WND / 2), (uint)pcb.mss)))
			{
				/* we can advertise more window */
				pcb.rcv_ann_wnd = pcb.rcv_wnd;
				return new_right_edge - pcb.rcv_ann_right_edge;
			}
			else
			{
				if (tcp.TCP_SEQ_GT(pcb.rcv_nxt, pcb.rcv_ann_right_edge))
				{
					/* Can happen due to other end sending out of advertised window,
					 * but within actual available (but not yet advertised) window */
					pcb.rcv_ann_wnd = 0;
				}
				else
				{
					/* keep the right edge of window constant */
					uint new_rcv_ann_wnd = pcb.rcv_ann_right_edge - pcb.rcv_nxt;
					lwip.LWIP_ASSERT("new_rcv_ann_wnd <= 0xffff", new_rcv_ann_wnd <= 0xffff);
					pcb.rcv_ann_wnd = (ushort)new_rcv_ann_wnd;
				}
				return 0;
			}
		}

		/**
		 * This function should be called by the application when it has
		 * processed the data. The purpose is to advertise a larger window
		 * when the data has been processed.
		 *
		 * @param pcb the tcp_pcb for which data is read
		 * @param len the amount of bytes that have been read by the application
		 */
		public void tcp_recved(tcp_pcb pcb, ushort len)
		{
			uint wnd_inflation;

			/* pcb.state tcp_state.LISTEN not allowed here */
			lwip.LWIP_ASSERT("don't call tcp_recved for listen-pcbs",
				pcb.state != tcp_state.LISTEN);
			lwip.LWIP_ASSERT("tcp_recved: len would wrap rcv_wnd\n",
				len <= 0xffff - pcb.rcv_wnd);

			pcb.rcv_wnd += len;
			if (pcb.rcv_wnd > opt.TCP_WND)
			{
				pcb.rcv_wnd = opt.TCP_WND;
			}

			wnd_inflation = tcp.tcp_update_rcv_ann_wnd(pcb);

			/* If the change in the right edge of window is significant (default
			 * watermark is opt.TCP_WND/4), then send an explicit update now.
			 * Otherwise wait for a packet to be sent in the normal course of
			 * events (or more window to be available later) */
			if (wnd_inflation >= opt.TCP_WND_UPDATE_THRESHOLD)
			{
				tcp.tcp_ack_now(pcb);
				tcp_output(pcb);
			}

			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_recved: recveived {0} bytes, wnd {1} ({2}).\n",
				   len, pcb.rcv_wnd, opt.TCP_WND - pcb.rcv_wnd);
		}

		/**
		 * Allocate a new local TCP port.
		 *
		 * @return a new (free) local TCP port number
		 */
		private ushort tcp_new_port()
		{
			byte i;
			ushort n = 0;
			tcp_pcb_common pcb;

		again:
			if (tcp_port++ == TCP_LOCAL_PORT_RANGE_END)
			{
				tcp_port = TCP_LOCAL_PORT_RANGE_START;
			}
			/* Check all PCB lists. */
			for (i = 0; i < NUM_TCP_PCB_LISTS; i++)
			{
				for (pcb = tcp_pcb_lists[i]; pcb != null; pcb = pcb.next)
				{
					if (pcb.local_port == tcp_port)
					{
						if (++n > (TCP_LOCAL_PORT_RANGE_END - TCP_LOCAL_PORT_RANGE_START))
						{
							return 0;
						}
						goto again;
					}
				}
			}
			return tcp_port;
		}

		/**
		 * Connects to another host. The function given as the "connected"
		 * argument will be called when the connection has been established.
		 *
		 * @param pcb the tcp_pcb used to establish the connection
		 * @param ipaddr the remote ip address to connect to
		 * @param port the remote tcp port to connect to
		 * @param connected callback function to call when connected (or on error)
		 * @return err_t.ERR_VAL if invalid arguments are given
		 *         err_t.ERR_OK if connect request has been sent
		 *         other err_t values if connect request couldn't be sent
		 */
		public err_t tcp_connect(tcp_pcb pcb, ip_addr ipaddr, ushort port,
			  tcp_connected_fn connected)
		{
			err_t ret;
			uint iss;
			ushort old_local_port;

			if (lwip.LWIP_ERROR("tcp_connect: can only connect from state tcp_state.CLOSED", pcb.state == tcp_state.CLOSED)) return err_t.ERR_ISCONN;

			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_connect to port {0}\n", port);
			if (ipaddr != null)
			{
				ip_addr.ip_addr_copy(pcb.remote_ip, ipaddr);
			}
			else
			{
				return err_t.ERR_VAL;
			}
			pcb.remote_port = port;

			/* check if we have a route to the remote host */
			if (ip_addr.ip_addr_isany(pcb.local_ip))
			{
				/* Use the netif's IP address as local address. */
				ip_addr.ip_addr_copy(pcb.local_ip, lwip.ip.ip_addr);
			}

			old_local_port = pcb.local_port;
			if (pcb.local_port == 0)
			{
				pcb.local_port = tcp_new_port();
				if (pcb.local_port == 0)
				{
					return err_t.ERR_BUF;
				}
			}
#if SO_REUSE
			if (ip.ip_get_option(pcb, sof.SOF_REUSEADDR))
			{
				/* Since sof.SOF_REUSEADDR allows reusing a local address, we have to make sure
				   now that the 5-tuple is unique. */
				tcp_pcb cpcb;
				int i;
				/* Don't check listen- and bound-PCBs, check active- and TIME-WAIT PCBs. */
				for (i = 2; i < NUM_TCP_PCB_LISTS; i++)
				{
					for (cpcb = (tcp_pcb)tcp_pcb_lists[i]; cpcb != null; cpcb = (tcp_pcb)cpcb.next)
					{
						if ((cpcb.local_port == pcb.local_port) &&
							(cpcb.remote_port == port) &&
							ip_addr.ip_addr_cmp(cpcb.local_ip, pcb.local_ip) &&
							ip_addr.ip_addr_cmp(cpcb.remote_ip, ipaddr))
						{
							/* linux returns EISCONN here, but err_t.ERR_USE should be OK for us */
							return err_t.ERR_USE;
						}
					}
				}
			}
#endif // SO_REUSE
			iss = tcp_next_iss();
			pcb.rcv_nxt = 0;
			pcb.snd_nxt = iss;
			pcb.lastack = iss - 1;
			pcb.snd_lbb = iss - 1;
			pcb.rcv_wnd = opt.TCP_WND;
			pcb.rcv_ann_wnd = opt.TCP_WND;
			pcb.rcv_ann_right_edge = pcb.rcv_nxt;
			pcb.snd_wnd = opt.TCP_WND;
			/* As initial send MSS, we use TCP_MSS but limit it to 536.
			   The send MSS is updated when an MSS option is received. */
			pcb.mss = (opt.TCP_MSS > 536) ? 536 : opt.TCP_MSS;
#if TCP_CALCULATE_EFF_SEND_MSS
			pcb.mss = tcp_eff_send_mss(pcb.mss, ipaddr);
#endif // TCP_CALCULATE_EFF_SEND_MSS
			pcb.cwnd = 1;
			pcb.ssthresh = (ushort)(pcb.mss * 10);
#if LWIP_CALLBACK_API
			pcb.connected = connected;
#else // LWIP_CALLBACK_API
			//LWIP_UNUSED_ARG(connected);
#endif // LWIP_CALLBACK_API

			/* Send a SYN together with the MSS option. */
			ret = tcp_enqueue_flags(pcb, tcp.TCP_SYN);
			if (ret == err_t.ERR_OK)
			{
				/* SYN segment was enqueued, changed the pcbs state now */
				pcb.state = tcp_state.SYN_SENT;
				if (old_local_port != 0)
				{
					TCP_RMV(ref tcp_bound_pcbs, pcb);
				}
				TCP_REG_ACTIVE(pcb);
				//snmp.snmp_inc_tcpactiveopens();

				tcp_output(pcb);
			}
			return ret;
		}

		/**
		 * Called every 500 ms and implements the retransmission timer and the timer that
		 * removes PCBs that have been in TIME-WAIT for enough time. It also increments
		 * various timers such as the inactivity timer in each PCB.
		 *
		 * Automatically called from tcp_tmr().
		 */
		public void tcp_slowtmr()
		{
			tcp_pcb pcb, prev;
			ushort eff_wnd;
			byte pcb_remove;      /* flag if a PCB should be removed */
			byte pcb_reset;       /* flag if a RST should be sent when removing */
			err_t err;

			err = err_t.ERR_OK;

			++tcp_ticks;
			++tcp_timer_ctr;

		tcp_slowtmr_start:
			/* Steps through all of the active PCBs. */
			prev = null;
			pcb = tcp_active_pcbs;
			if (pcb == null)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: no active pcbs\n");
			}
			while (pcb != null)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: processing active pcb\n");
				lwip.LWIP_ASSERT("tcp_slowtmr: active pcb.state != tcp_state.CLOSED\n", pcb.state != tcp_state.CLOSED);
				lwip.LWIP_ASSERT("tcp_slowtmr: active pcb.state != tcp_state.LISTEN\n", pcb.state != tcp_state.LISTEN);
				lwip.LWIP_ASSERT("tcp_slowtmr: active pcb.state != TIME-WAIT\n", pcb.state != tcp_state.TIME_WAIT);
				if (pcb.last_timer == tcp_timer_ctr)
				{
					/* skip this pcb, we have already processed it */
					pcb = (tcp_pcb)pcb.next;
					continue;
				}
				pcb.last_timer = tcp_timer_ctr;

				pcb_remove = 0;
				pcb_reset = 0;

				if (pcb.state == tcp_state.SYN_SENT && pcb.nrtx == opt.TCP_SYNMAXRTX)
				{
					++pcb_remove;
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: max SYN retries reached\n");
				}
				else if (pcb.nrtx == opt.TCP_MAXRTX)
				{
					++pcb_remove;
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: max DATA retries reached\n");
				}
				else
				{
					if (pcb.persist_backoff > 0)
					{
						/* If snd_wnd is zero, use persist timer to send 1 byte probes
						 * instead of using the standard retransmission mechanism. */
						pcb.persist_cnt++;
						if (pcb.persist_cnt >= tcp_persist_backoff[pcb.persist_backoff - 1])
						{
							pcb.persist_cnt = 0;
							if (pcb.persist_backoff < tcp_persist_backoff.Length)
							{
								pcb.persist_backoff++;
							}
							tcp_zero_window_probe(pcb);
						}
					}
					else
					{
						/* Increase the retransmission timer if it is running */
						if (pcb.rtime >= 0)
						{
							++pcb.rtime;
						}

						if (pcb.unacked != null && pcb.rtime >= pcb.rto)
						{
							/* Time for a retransmission. */
							lwip.LWIP_DEBUGF(opt.TCP_RTO_DEBUG, "tcp_slowtmr: rtime {0}"
														+ " pcb.rto {1}\n",
														pcb.rtime, pcb.rto);

							/* Double retransmission time-out unless we are trying to
							 * connect to somebody (i.e., we are in tcp_state.SYN_SENT). */
							if (pcb.state != tcp_state.SYN_SENT)
							{
								pcb.rto = (short)(((pcb.sa >> 3) + pcb.sv) << tcp_backoff[pcb.nrtx]);
							}

							/* Reset the retransmission timer. */
							pcb.rtime = 0;

							/* Reduce congestion window and ssthresh. */
							eff_wnd = Math.Min(pcb.cwnd, pcb.snd_wnd);
							pcb.ssthresh = (ushort)(eff_wnd >> 1);
							if (pcb.ssthresh < (pcb.mss << 1))
							{
								pcb.ssthresh = (ushort)(pcb.mss << 1);
							}
							pcb.cwnd = pcb.mss;
							lwip.LWIP_DEBUGF(opt.TCP_CWND_DEBUG, "tcp_slowtmr: cwnd {0}"
														 + " ssthresh {1}\n",
														 pcb.cwnd, pcb.ssthresh);

							/* The following needs to be called AFTER cwnd is set to one
							   mss - STJ */
							tcp_rexmit_rto(pcb);
						}
					}
				}
				/* Check if this PCB has stayed too long in FIN-WAIT-2 */
				if (pcb.state == tcp_state.FIN_WAIT_2)
				{
					/* If this PCB is in tcp_state.FIN_WAIT_2 because of SHUT_WR don't let it time out. */
					if ((pcb.flags & tcp_pcb.TF_RXCLOSED) != 0)
					{
						/* PCB was fully closed (either through close() or SHUT_RDWR):
						   normal FIN-WAIT timeout handling. */
						if ((uint)(tcp_ticks - pcb.tmr) >
							tcp.TCP_FIN_WAIT_TIMEOUT / tcp.TCP_SLOW_INTERVAL)
						{
							++pcb_remove;
							lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: removing pcb stuck in FIN-WAIT-2\n");
						}
					}
				}

				/* Check if KEEPALIVE should be sent */
				if (ip.ip_get_option(pcb, (byte)sof.SOF_KEEPALIVE) &&
				   ((pcb.state == tcp_state.ESTABLISHED) ||
					(pcb.state == tcp_state.CLOSE_WAIT)))
				{
					if ((uint)(tcp_ticks - pcb.tmr) >
					   (pcb.keep_idle + TCP_KEEP_DUR(pcb)) / tcp.TCP_SLOW_INTERVAL)
					{
						lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: KEEPALIVE timeout. Aborting connection to {0}.{1}.{2}.{3}.\n",
												ip_addr.ip4_addr1_16(pcb.remote_ip), ip_addr.ip4_addr2_16(pcb.remote_ip),
												ip_addr.ip4_addr3_16(pcb.remote_ip), ip_addr.ip4_addr4_16(pcb.remote_ip));

						++pcb_remove;
						++pcb_reset;
					}
					else if ((uint)(tcp_ticks - pcb.tmr) >
							(pcb.keep_idle + pcb.keep_cnt_sent * TCP_KEEP_INTVL(pcb))
							/ tcp.TCP_SLOW_INTERVAL)
					{
						tcp_keepalive(pcb);
						pcb.keep_cnt_sent++;
					}
				}

				/* If this PCB has queued out of sequence data, but has been
				   inactive for too long, will drop the data (it will eventually
				   be retransmitted). */
#if TCP_QUEUE_OOSEQ
				if (pcb.ooseq != null &&
					(uint)tcp_ticks - pcb.tmr >= pcb.rto * TCP_OOSEQ_TIMEOUT)
				{
					tcp_segs_free(pcb.ooseq);
					pcb.ooseq = null;
					lwip.LWIP_DEBUGF(opt.TCP_CWND_DEBUG, "tcp_slowtmr: dropping OOSEQ queued data\n");
				}
#endif // TCP_QUEUE_OOSEQ

				/* Check if this PCB has stayed too long in SYN-RCVD */
				if (pcb.state == tcp_state.SYN_RCVD)
				{
					if ((uint)(tcp_ticks - pcb.tmr) >
						tcp.TCP_SYN_RCVD_TIMEOUT / tcp.TCP_SLOW_INTERVAL)
					{
						++pcb_remove;
						lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: removing pcb stuck in SYN-RCVD\n");
					}
				}

				/* Check if this PCB has stayed too long in LAST-ACK */
				if (pcb.state == tcp_state.LAST_ACK)
				{
					if ((uint)(tcp_ticks - pcb.tmr) > 2 * tcp.TCP_MSL / tcp.TCP_SLOW_INTERVAL)
					{
						++pcb_remove;
						lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: removing pcb stuck in LAST-ACK\n");
					}
				}

				/* If the PCB should be removed, do it. */
				if (pcb_remove != 0)
				{
					tcp_pcb pcb2;
					tcp_err_fn err_fn;
					object err_arg;
					tcp_pcb_purge(pcb);
					/* Remove PCB from tcp.tcp_active_pcbs list. */
					if (prev != null)
					{
						lwip.LWIP_ASSERT("tcp_slowtmr: middle tcp != tcp.tcp_active_pcbs", pcb != tcp_active_pcbs);
						prev.next = pcb.next;
					}
					else
					{
						/* This PCB was the first. */
						lwip.LWIP_ASSERT("tcp_slowtmr: first pcb == tcp.tcp_active_pcbs", tcp_active_pcbs == pcb);
						tcp_active_pcbs = (tcp_pcb)pcb.next;
					}

					if (pcb_reset != 0)
					{
						tcp_rst(pcb.snd_nxt, pcb.rcv_nxt, pcb.local_ip, pcb.remote_ip,
							pcb.local_port, pcb.remote_port);
					}

					err_fn = pcb.errf;
					err_arg = pcb.callback_arg;
					pcb2 = pcb;
					pcb = (tcp_pcb)pcb.next;
					lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb2);

					tcp_active_pcbs_changed = 0;
					TCP_EVENT_ERR(err_fn, err_arg, err_t.ERR_ABRT);
					if (tcp_active_pcbs_changed != 0)
					{
						goto tcp_slowtmr_start;
					}
				}
				else
				{
					/* get the 'next' element now and work with 'prev' below (in case of abort) */
					prev = pcb;
					pcb = (tcp_pcb)pcb.next;

					/* We check if we should poll the connection. */
					++prev.polltmr;
					if (prev.polltmr >= prev.pollinterval)
					{
						prev.polltmr = 0;
						lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_slowtmr: polling application\n");
						tcp_active_pcbs_changed = 0;
						TCP_EVENT_POLL(prev, out err);
						if (tcp_active_pcbs_changed != 0)
						{
							goto tcp_slowtmr_start;
						}
						/* if err == err_t.ERR_ABRT, 'prev' is already deallocated */
						if (err == err_t.ERR_OK)
						{
							tcp_output(prev);
						}
					}
				}
			}


			/* Steps through all of the TIME-WAIT PCBs. */
			prev = null;
			pcb = tcp_tw_pcbs;
			while (pcb != null)
			{
				lwip.LWIP_ASSERT("tcp_slowtmr: TIME-WAIT pcb.state == TIME-WAIT", pcb.state == tcp_state.TIME_WAIT);
				pcb_remove = 0;

				/* Check if this PCB has stayed long enough in TIME-WAIT */
				if ((uint)(tcp_ticks - pcb.tmr) > 2 * tcp.TCP_MSL / tcp.TCP_SLOW_INTERVAL)
				{
					++pcb_remove;
				}



				/* If the PCB should be removed, do it. */
				if (pcb_remove != 0)
				{
					tcp_pcb pcb2;
					tcp_pcb_purge(pcb);
					/* Remove PCB from tcp_tw_pcbs list. */
					if (prev != null)
					{
						lwip.LWIP_ASSERT("tcp_slowtmr: middle tcp != tcp_tw_pcbs", pcb != tcp_tw_pcbs);
						prev.next = pcb.next;
					}
					else
					{
						/* This PCB was the first. */
						lwip.LWIP_ASSERT("tcp_slowtmr: first pcb == tcp_tw_pcbs", tcp_tw_pcbs == pcb);
						tcp_tw_pcbs = (tcp_pcb)pcb.next;
					}
					pcb2 = pcb;
					pcb = (tcp_pcb)pcb.next;
					lwip.memp_free(memp_t.MEMP_TCP_PCB, pcb2);
				}
				else
				{
					prev = pcb;
					pcb = (tcp_pcb)pcb.next;
				}
			}
		}

		/**
		 * Is called every TCP_FAST_INTERVAL (250 ms) and process data previously
		 * "refused" by upper layer (application) and sends delayed ACKs.
		 *
		 * Automatically called from tcp_tmr().
		 */
		public void tcp_fasttmr()
		{
			tcp_pcb pcb;

			++tcp_timer_ctr;

		tcp_fasttmr_start:
			pcb = tcp_active_pcbs;

			while (pcb != null)
			{
				if (pcb.last_timer != tcp_timer_ctr)
				{
					tcp_pcb next;
					pcb.last_timer = tcp_timer_ctr;
					/* send delayed ACKs */
					if ((pcb.flags & tcp_pcb.TF_ACK_DELAY) != 0)
					{
						lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_fasttmr: delayed ACK\n");
						tcp.tcp_ack_now(pcb);
						tcp_output(pcb);
						pcb.flags &= unchecked((byte)~(tcp_pcb.TF_ACK_DELAY | tcp_pcb.TF_ACK_NOW));
					}

					next = (tcp_pcb)pcb.next;

					/* If there is data which was previously "refused" by upper layer */
					if (pcb.refused_data != null)
					{
						tcp_active_pcbs_changed = 0;
						tcp_process_refused_data(pcb);
						if (tcp_active_pcbs_changed != 0)
						{
							/* application callback has changed the pcb list: restart the loop */
							goto tcp_fasttmr_start;
						}
					}
					pcb = next;
				}
			}
		}

		/** Pass pcb.refused_data to the recv callback */
		public err_t tcp_process_refused_data(tcp_pcb pcb)
		{
			err_t err;
			byte refused_flags = pcb.refused_data.flags;
			/* set pcb.refused_data to null in case the callback frees it and then
			   closes the pcb */
			pbuf refused_data = pcb.refused_data;
			pcb.refused_data = null;
			/* Notify again application with data previously received. */
			lwip.LWIP_DEBUGF(opt.TCP_INPUT_DEBUG, "tcp_input: notify kept packet\n");
			TCP_EVENT_RECV(pcb, refused_data, err_t.ERR_OK, out err);
			if (err == err_t.ERR_OK)
			{
				/* did refused_data include a FIN? */
				if ((refused_flags & pbuf.PBUF_FLAG_TCP_FIN) != 0)
				{
					/* correct rcv_wnd as the application won't call tcp_recved()
					   for the FIN's seqno */
					if (pcb.rcv_wnd != opt.TCP_WND)
					{
						pcb.rcv_wnd++;
					}
					TCP_EVENT_CLOSED(pcb, out err);
					if (err == err_t.ERR_ABRT)
					{
						return err_t.ERR_ABRT;
					}
				}
			}
			else if (err == err_t.ERR_ABRT)
			{
				/* if err == err_t.ERR_ABRT, 'pcb' is already deallocated */
				/* Drop incoming packets because pcb is "full" (only if the incoming
				   segment contains data). */
				lwip.LWIP_DEBUGF(opt.TCP_INPUT_DEBUG, "tcp_input: drop incoming packets, because pcb is \"full\"\n");
				return err_t.ERR_ABRT;
			}
			else
			{
				/* data is still refused, pbuf is still valid (go on for ACK-only packets) */
				pcb.refused_data = refused_data;
			}
			return err_t.ERR_OK;
		}

		/**
		 * Deallocates a list of TCP segments (tcp_seg structures).
		 *
		 * @param seg tcp_seg list of TCP segments to free
		 */
		public void tcp_segs_free(tcp_seg seg)
		{
			while (seg != null)
			{
				tcp_seg next = seg.next;
				tcp_seg_free(seg);
				seg = next;
			}
		}

		/**
		 * Frees a TCP segment (tcp_seg structure).
		 *
		 * @param seg single tcp_seg to free
		 */
		public void tcp_seg_free(tcp_seg seg)
		{
			if (seg != null)
			{
				if (seg.p != null)
				{
					lwip.pbuf_free(seg.p);
#if TCP_DEBUG
					seg.p = null;
#endif // TCP_DEBUG
				}
				lwip.memp_free(memp_t.MEMP_TCP_SEG, seg);
			}
		}

		/**
		 * Sets the priority of a connection.
		 *
		 * @param pcb the tcp_pcb to manipulate
		 * @param prio new priority
		 */
		public static void tcp_setprio(tcp_pcb pcb, byte prio)
		{
			pcb.prio = prio;
		}

#if TCP_QUEUE_OOSEQ
		/**
		 * Returns a copy of the given TCP segment.
		 * The pbuf and data are not copied, only the pointers
		 *
		 * @param seg the old tcp_seg
		 * @return a copy of seg
		 */
		public tcp_seg tcp_seg_copy(tcp_seg seg)
		{
			tcp_seg cseg;

			cseg = (tcp_seg)lwip.memp_malloc(memp_t.MEMP_TCP_SEG);
			if (cseg == null)
			{
				return null;
			}
			//opt.SMEMCPY(cseg, seg, tcp_seg.length);
			cseg.copy_from(seg);
			lwip.pbuf_ref(cseg.p);
			return cseg;
		}
#endif // TCP_QUEUE_OOSEQ

#if LWIP_CALLBACK_API
		/**
		 * Default receive callback that is called if the user didn't register
		 * a recv callback for the pcb.
		 */
		public err_t tcp_recv_null(object arg, tcp_pcb pcb, pbuf p, err_t err)
		{
			//LWIP_UNUSED_ARG(arg);
			if (p != null)
			{
				tcp_recved(pcb, p.tot_len);
				lwip.pbuf_free(p);
			}
			else if (err == err_t.ERR_OK)
			{
				return tcp_close(pcb);
			}
			return err_t.ERR_OK;
		}
#endif // LWIP_CALLBACK_API

		/**
		 * Kills the oldest active connection that has the same or lower priority than
		 * 'prio'.
		 *
		 * @param prio minimum priority
		 */
		private void tcp_kill_prio(byte prio)
		{
			tcp_pcb pcb, inactive;
			uint inactivity;
			byte mprio;


			mprio = TCP_PRIO_MAX;

			/* We kill the oldest active connection that has lower priority than prio. */
			inactivity = 0;
			inactive = null;
			for (pcb = tcp_active_pcbs; pcb != null; pcb = (tcp_pcb)pcb.next)
			{
				if (pcb.prio <= prio &&
					pcb.prio <= mprio &&
					(uint)(tcp_ticks - pcb.tmr) >= inactivity)
				{
					inactivity = tcp_ticks - pcb.tmr;
					inactive = pcb;
					mprio = pcb.prio;
				}
			}
			if (inactive != null)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_kill_prio: killing oldest PCB {0} ({1})\n",
					   inactive, inactivity);
				tcp_abort(inactive);
			}
		}

		/**
		 * Kills the oldest connection that is in tcp_state.TIME_WAIT state.
		 * Called from tcp_alloc() if no more connections are available.
		 */
		private void tcp_kill_timewait()
		{
			tcp_pcb pcb, inactive;
			uint inactivity;

			inactivity = 0;
			inactive = null;
			/* Go through the list of tcp_state.TIME_WAIT pcbs and get the oldest pcb. */
			for (pcb = tcp_tw_pcbs; pcb != null; pcb = (tcp_pcb)pcb.next)
			{
				if ((uint)(tcp_ticks - pcb.tmr) >= inactivity)
				{
					inactivity = tcp_ticks - pcb.tmr;
					inactive = pcb;
				}
			}
			if (inactive != null)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_kill_timewait: killing oldest TIME-WAIT PCB {0} ({1})\n",
					   inactive, inactivity);
				tcp_abort(inactive);
			}
		}

		/**
		 * Allocate a new tcp_pcb structure.
		 *
		 * @param prio priority for the new pcb
		 * @return a new tcp_pcb that initially is in state tcp_state.CLOSED
		 */
		public tcp_pcb tcp_alloc(byte prio)
		{
			tcp_pcb pcb;
			uint iss;

			pcb = (tcp_pcb)lwip.memp_malloc(memp_t.MEMP_TCP_PCB);
			if (pcb == null)
			{
				/* Try killing oldest connection in TIME-WAIT. */
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_alloc: killing off oldest TIME-WAIT connection\n");
				tcp_kill_timewait();
				/* Try to allocate a tcp_pcb again. */
				pcb = (tcp_pcb)lwip.memp_malloc(memp_t.MEMP_TCP_PCB);
				if (pcb == null)
				{
					/* Try killing active connections with lower priority than the new one. */
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_alloc: killing connection with prio lower than {0}\n", prio);
					tcp_kill_prio(prio);
					/* Try to allocate a tcp_pcb again. */
					pcb = (tcp_pcb)lwip.memp_malloc(memp_t.MEMP_TCP_PCB);
					if (pcb != null)
					{
						/* adjust err stats: lwip.memp_malloc failed twice before */
						--lwip.lwip_stats.memp[(int)memp_t.MEMP_TCP_PCB].err;
					}
				}
				if (pcb != null)
				{
					/* adjust err stats: timewait PCB was freed above */
					--lwip.lwip_stats.memp[(int)memp_t.MEMP_TCP_PCB].err;
				}
			}
			if (pcb != null)
			{
				memp.memset(pcb, 0, tcp_pcb.length);
				pcb.prio = prio;
				pcb.snd_buf = opt.TCP_SND_BUF;
				pcb.snd_queuelen = 0;
				pcb.rcv_wnd = opt.TCP_WND;
				pcb.rcv_ann_wnd = opt.TCP_WND;
				pcb.tos = 0;
				pcb.ttl = opt.TCP_TTL;
				/* As initial send MSS, we use TCP_MSS but limit it to 536.
				   The send MSS is updated when an MSS option is received. */
				pcb.mss = (opt.TCP_MSS > 536) ? 536 : opt.TCP_MSS;
				pcb.rto = 3000 / tcp.TCP_SLOW_INTERVAL;
				pcb.sa = 0;
				pcb.sv = 3000 / tcp.TCP_SLOW_INTERVAL;
				pcb.rtime = -1;
				pcb.cwnd = 1;
				iss = tcp_next_iss();
				pcb.snd_wl2 = iss;
				pcb.snd_nxt = iss;
				pcb.lastack = iss;
				pcb.snd_lbb = iss;
				pcb.tmr = tcp_ticks;
				pcb.last_timer = tcp_timer_ctr;

				pcb.polltmr = 0;

#if LWIP_CALLBACK_API
				pcb.recv = tcp_recv_null;
#endif // LWIP_CALLBACK_API

				/* Init KEEPALIVE timer */
				pcb.keep_idle = tcp.TCP_KEEPIDLE_DEFAULT;

#if LWIP_TCP_KEEPALIVE
				pcb.keep_intvl = TCP_KEEPINTVL_DEFAULT;
				pcb.keep_cnt = TCP_KEEPCNT_DEFAULT;
#endif // LWIP_TCP_KEEPALIVE

				pcb.keep_cnt_sent = 0;
			}
			return pcb;
		}

		/**
		 * Creates a new TCP protocol control block but doesn't place it on
		 * any of the TCP PCB lists.
		 * The pcb is not put on any list until binding using tcp_bind().
		 *
		 * @internal: Maybe there should be a idle TCP PCB list where these
		 * PCBs are put on. Port reservation using tcp_bind() is implemented but
		 * allocated pcbs that are not bound can't be killed automatically if wanting
		 * to allocate a pcb with higher prio (@see tcp_kill_prio())
		 *
		 * @return a new tcp_pcb that initially is in state tcp_state.CLOSED
		 */
		public tcp_pcb tcp_new()
		{
			return tcp_alloc(TCP_PRIO_NORMAL);
		}

		/**
		 * Used to specify the argument that should be passed callback
		 * functions.
		 *
		 * @param pcb tcp_pcb to set the callback argument
		 * @param arg void pointer argument to pass to callback functions
		 */
		public static void tcp_arg(tcp_pcb pcb, object arg)
		{
			/* This function is allowed to be called for both listen pcbs and
			   connection pcbs. */
			pcb.callback_arg = arg;
		}
#if LWIP_CALLBACK_API

		/**
		 * Used to specify the function that should be called when a TCP
		 * connection receives data.
		 *
		 * @param pcb tcp_pcb to set the recv callback
		 * @param recv callback function to call for this pcb when data is received
		 */
		public static void tcp_recv(tcp_pcb pcb, tcp_recv_fn recv)
		{
			lwip.LWIP_ASSERT("invalid socket state for recv callback", pcb.state != tcp_state.LISTEN);
			pcb.recv = recv;
		}

		/**
		 * Used to specify the function that should be called when TCP data
		 * has been successfully delivered to the remote host.
		 *
		 * @param pcb tcp_pcb to set the sent callback
		 * @param sent callback function to call for this pcb when data is successfully sent
		 */
		public static void tcp_sent(tcp_pcb pcb, tcp_sent_fn sent)
		{
			lwip.LWIP_ASSERT("invalid socket state for sent callback", pcb.state != tcp_state.LISTEN);
			pcb.sent = sent;
		}

		/**
		 * Used to specify the function that should be called when a fatal error
		 * has occured on the connection.
		 *
		 * @param pcb tcp_pcb to set the err callback
		 * @param err callback function to call for this pcb when a fatal error
		 *        has occured on the connection
		 */
		public static void tcp_err(tcp_pcb pcb, tcp_err_fn err)
		{
			lwip.LWIP_ASSERT("invalid socket state for err callback", pcb.state != tcp_state.LISTEN);
			pcb.errf = err;
		}

		/**
		 * Used for specifying the function that should be called when a
		 * tcp_state.LISTENing connection has been connected to another host.
		 *
		 * @param pcb tcp_pcb to set the accept callback
		 * @param accept callback function to call for this pcb when tcp_state.LISTENing
		 *        connection has been connected to another host
		 */
		public static void tcp_accept(tcp_pcb_common pcb, tcp_accept_fn accept)
		{
			/* This function is allowed to be called for both listen pcbs and
			   connection pcbs. */
			pcb.accept = accept;
		}
#endif // LWIP_CALLBACK_API


		/**
		 * Used to specify the function that should be called periodically
		 * from TCP. The interval is specified in terms of the TCP coarse
		 * timer interval, which is called twice a second.
		 *
		 */
		public static void tcp_poll(tcp_pcb pcb, tcp_poll_fn poll, byte interval)
		{
			lwip.LWIP_ASSERT("invalid socket state for poll", pcb.state != tcp_state.LISTEN);
#if LWIP_CALLBACK_API
			pcb.poll = poll;
#else // LWIP_CALLBACK_API
			//LWIP_UNUSED_ARG(poll);
#endif // LWIP_CALLBACK_API
			pcb.pollinterval = interval;
		}

		/**
		 * Purges a TCP PCB. Removes any buffered data and frees the buffer memory
		 * (pcb.ooseq, pcb.unsent and pcb.unacked are freed).
		 *
		 * @param pcb tcp_pcb to purge. The pcb itself is not deallocated!
		 */
		public void tcp_pcb_purge(tcp_pcb_common _pcb)
		{
			if (_pcb.state != tcp_state.CLOSED &&
				_pcb.state != tcp_state.TIME_WAIT &&
				_pcb.state != tcp_state.LISTEN)
			{
				tcp_pcb pcb = (tcp_pcb)_pcb;
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_pcb_purge\n");

#if TCP_LISTEN_BACKLOG
				if (pcb.state == tcp_state.SYN_RCVD)
				{
					/* Need to find the corresponding listen_pcb and decrease its accepts_pending */
					tcp_pcb_listen lpcb;
					lwip.LWIP_ASSERT("tcp_pcb_purge: pcb.state == tcp_state.SYN_RCVD but tcp_listen_pcbs is null",
					tcp_listen_pcbs.listen_pcbs != null);
					for (lpcb = tcp_listen_pcbs.listen_pcbs; lpcb != null; lpcb = (tcp_pcb_listen)lpcb.next)
					{
						if ((lpcb.local_port == pcb.local_port) &&
							(ip_addr.ip_addr_isany(lpcb.local_ip) ||
								ip_addr.ip_addr_cmp(pcb.local_ip, lpcb.local_ip)))
						{
							/* port and address of the listen pcb match the timed-out pcb */
							lwip.LWIP_ASSERT("tcp_pcb_purge: listen pcb does not have accepts pending",
								lpcb.accepts_pending > 0);
							lpcb.accepts_pending--;
							break;
						}
					}
				}
#endif // TCP_LISTEN_BACKLOG

				if (pcb.refused_data != null)
				{
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_pcb_purge: data left on .refused_data\n");
					lwip.pbuf_free(pcb.refused_data);
					pcb.refused_data = null;
				}
				if (pcb.unsent != null)
				{
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_pcb_purge: not all data sent\n");
				}
				if (pcb.unacked != null)
				{
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_pcb_purge: data left on .unacked\n");
				}
#if TCP_QUEUE_OOSEQ
				if (pcb.ooseq != null)
				{
					lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "tcp_pcb_purge: data left on .ooseq\n");
				}
				tcp_segs_free(pcb.ooseq);
				pcb.ooseq = null;
#endif // TCP_QUEUE_OOSEQ

				/* Stop the retransmission timer as it will expect data on unacked
				   queue if it fires */
				pcb.rtime = -1;

				tcp_segs_free(pcb.unsent);
				tcp_segs_free(pcb.unacked);
				pcb.unacked = pcb.unsent = null;
#if TCP_OVERSIZE
				pcb.unsent_oversize = 0;
#endif // TCP_OVERSIZE
			}
		}

		/**
		 * Purges the PCB and removes it from a PCB list. Any delayed ACKs are sent first.
		 *
		 * @param pcblist PCB list to purge.
		 * @param pcb tcp_pcb to purge. The pcb itself is NOT deallocated!
		 */
		public void tcp_pcb_remove(tcp_pcb_common pcblist, tcp_pcb_common pcb)
		{
			TCP_RMV(ref pcblist, pcb);

			tcp_pcb_purge(pcb);

			if (pcb.state != tcp_state.LISTEN)
			{
				tcp_pcb _pcb = (tcp_pcb)pcb;
				/* if there is an outstanding delayed ACKs, send it */
				if (_pcb.state != tcp_state.TIME_WAIT &&
					(_pcb.flags & tcp_pcb.TF_ACK_DELAY) != 0)
				{
					_pcb.flags |= tcp_pcb.TF_ACK_NOW;
					tcp_output(_pcb);
				}

				lwip.LWIP_ASSERT("unsent segments leaking", _pcb.unsent == null);
				lwip.LWIP_ASSERT("unacked segments leaking", _pcb.unacked == null);
#if TCP_QUEUE_OOSEQ
				lwip.LWIP_ASSERT("ooseq segments leaking", _pcb.ooseq == null);
#endif // TCP_QUEUE_OOSEQ
			}

			pcb.state = tcp_state.CLOSED;

			lwip.LWIP_ASSERT("tcp_pcb_remove: tcp_pcbs_sane()", tcp_pcbs_sane() != 0);
		}

		uint iss = 6510;

		/**
		 * Calculates a new initial sequence number for new connections.
		 *
		 * @return uint pseudo random sequence number
		 */
		public uint tcp_next_iss()
		{
			iss += tcp_ticks;       /* XXX */
			return iss;
		}

#if TCP_CALCULATE_EFF_SEND_MSS
		/**
		 * Calcluates the effective send mss that can be used for a specific IP address
		 * by using ip.ip_route to determin the netif used to send to the address and
		 * calculating the minimum of TCP_MSS and that netif's mtu (if set).
		 */
		public ushort tcp_eff_send_mss(ushort sendmss, ip_addr addr)
		{
			ushort mss_s;

			mss_s = (ushort)(lwip.ip.mtu - ip.IP_HLEN - tcp.TCP_HLEN);
			/* RFC 1122, chap 4.2.2.6:
			 * Eff.snd.MSS = min(SendMSS+20, MMS_S) - TCPhdrsize - IPoptionsize
			 * We correct for TCP options in tcp_write(), and don't support IP options.
			 */
			sendmss = Math.Min(sendmss, mss_s);

			return sendmss;
		}
#endif // TCP_CALCULATE_EFF_SEND_MSS

		public static string tcp_debug_state_str(tcp_state s)
		{
			return tcp_state_str[(int)s];
		}

#if TCP_DEBUG || TCP_INPUT_DEBUG || TCP_OUTPUT_DEBUG
		/**
		 * Print a tcp header for debugging purposes.
		 *
		 * @param tcphdr pointer to a tcp_hdr
		 */
		public static void tcp_debug_print(tcp_hdr tcphdr)
		{
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "TCP header:\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "|    {0,5}      |    {1,5}      | (src port, dest port)\n",
				lwip.lwip_ntohs(tcphdr.src), lwip.lwip_ntohs(tcphdr.dest));
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "|           {0:0000000000}          | (seq no)\n",
				lwip.lwip_ntohl(tcphdr.seqno));
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "|           {0:0000000000}          | (ack no)\n",
				lwip.lwip_ntohl(tcphdr.ackno));
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "| {0,2} |   |{1}{2}{3}{4}{5}{6}|     {7,5}     | (hdrlen, flags (",
				tcp_hdr.TCPH_HDRLEN(tcphdr),
				tcp_hdr.TCPH_FLAGS(tcphdr) >> 5 & 1,
				tcp_hdr.TCPH_FLAGS(tcphdr) >> 4 & 1,
				tcp_hdr.TCPH_FLAGS(tcphdr) >> 3 & 1,
				tcp_hdr.TCPH_FLAGS(tcphdr) >> 2 & 1,
				tcp_hdr.TCPH_FLAGS(tcphdr) >> 1 & 1,
				tcp_hdr.TCPH_FLAGS(tcphdr) & 1,
				lwip.lwip_ntohs(tcphdr.wnd));
			tcp_debug_print_flags(tcp_hdr.TCPH_FLAGS(tcphdr));
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "), win)\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "|    0x{0:X4}     |     {1,5}     | (chksum, urgp)\n",
				lwip.lwip_ntohs(tcphdr.chksum), lwip.lwip_ntohs(tcphdr.urgp));
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "+-------------------------------+\n");
		}

		/**
		 * Print a tcp state for debugging purposes.
		 *
		 * @param s tcp_state to print
		 */
		public static void tcp_debug_print_state(tcp_state s)
		{
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "State: {0}\n", tcp_state_str[(int)s]);
		}

		/**
		 * Print tcp flags for debugging purposes.
		 *
		 * @param flags tcp flags, all active flags are printed
		 */
		public static void tcp_debug_print_flags(byte flags)
		{
			if ((flags & tcp.TCP_FIN) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "FIN ");
			}
			if ((flags & tcp.TCP_SYN) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "SYN ");
			}
			if ((flags & tcp.TCP_RST) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "RST ");
			}
			if ((flags & tcp.TCP_PSH) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "PSH ");
			}
			if ((flags & tcp.TCP_ACK) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "ACK ");
			}
			if ((flags & tcp.TCP_URG) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "URG ");
			}
			if ((flags & tcp.TCP_ECE) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "ECE ");
			}
			if ((flags & tcp.TCP_CWR) != 0)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "CWR ");
			}
			//lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "\n");
		}

		/**
		 * Print all tcp_pcbs in every list for debugging purposes.
		 */
		public void tcp_debug_print_pcbs()
		{
			tcp_pcb pcb;
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "Active PCB states:\n");
			for (pcb = tcp_active_pcbs; pcb != null; pcb = (tcp_pcb)pcb.next)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "Local port {0}, foreign port {1} snd_nxt {2} rcv_nxt {3} ",
								   pcb.local_port, pcb.remote_port,
								   pcb.snd_nxt, pcb.rcv_nxt);
				tcp_debug_print_state(pcb.state);
			}
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "Listen PCB states:\n");
			for (pcb = (tcp_pcb)tcp_listen_pcbs.pcbs; pcb != null; pcb = (tcp_pcb)pcb.next)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "Local port {0}, foreign port {1} snd_nxt {2} rcv_nxt {3} ",
								   pcb.local_port, pcb.remote_port,
								   pcb.snd_nxt, pcb.rcv_nxt);
				tcp_debug_print_state(pcb.state);
			}
			lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "TIME-WAIT PCB states:\n");
			for (pcb = tcp_tw_pcbs; pcb != null; pcb = (tcp_pcb)pcb.next)
			{
				lwip.LWIP_DEBUGF(opt.TCP_DEBUG, "Local port {0}, foreign port {1} snd_nxt {2} rcv_nxt {3} ",
								   pcb.local_port, pcb.remote_port,
								   pcb.snd_nxt, pcb.rcv_nxt);
				tcp_debug_print_state(pcb.state);
			}
		}
#endif // TCP_DEBUG
		/**
		 * Check state consistency of the tcp_pcb lists.
		 */
		public short tcp_pcbs_sane()
		{
			tcp_pcb_common pcb;
			for (pcb = tcp_active_pcbs; pcb != null; pcb = pcb.next)
			{
				lwip.LWIP_ASSERT("tcp_pcbs_sane: active pcb.state != tcp_state.CLOSED", pcb.state != tcp_state.CLOSED);
				lwip.LWIP_ASSERT("tcp_pcbs_sane: active pcb.state != tcp_state.LISTEN", pcb.state != tcp_state.LISTEN);
				lwip.LWIP_ASSERT("tcp_pcbs_sane: active pcb.state != TIME-WAIT", pcb.state != tcp_state.TIME_WAIT);
			}
			for (pcb = tcp_tw_pcbs; pcb != null; pcb = pcb.next)
			{
				lwip.LWIP_ASSERT("tcp_pcbs_sane: tw pcb.state == TIME-WAIT", pcb.state == tcp_state.TIME_WAIT);
			}
			return 1;
		}
	}

	partial class lwip
	{
		internal tcp tcp;
	}
}
