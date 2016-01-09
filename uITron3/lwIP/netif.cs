using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lwIP
{
	public delegate void netif_output_t(netif netif, byte[] packet, ip_addr src, ip_addr dest, byte proto);

	public class netif
	{
		lwip lwip;

		public const byte NETIF_FLAG_BROADCAST = (byte)0x02U;

		public ushort mtu = 1500;
		public ip_addr ip_addr = new ip_addr(new byte[ip_addr.length]);
		public ip_addr netmask = new ip_addr(new byte[ip_addr.length]);
		public ip_addr gw = new ip_addr(new byte[ip_addr.length]);
		public byte flags = NETIF_FLAG_BROADCAST;
		private netif_output_t m_output;
		private object addr_hint;
		public eth_addr ethaddr;
		public string name;

		public netif(lwip lwip, string name, ip_addr ipaddr, ip_addr netmask, ip_addr gw, netif_output_t output)
		{
			this.lwip = lwip;
			this.name = name;
			this.m_output = output;
			set_ipaddr(ipaddr);
			set_netmask(netmask);
			set_gw(gw);
		}

		internal static void netif_init(lwip lwip, string name, ip_addr ipaddr, ip_addr netmask, ip_addr gw, netif_output_t output)
		{
			lwip.netif = new netif(lwip, name, ipaddr, netmask, gw, output);
		}

		private void set_ipaddr(ip_addr ipaddr)
		{
			ip_addr.ip_addr_set(this.ip_addr, ipaddr);

			lwip.LWIP_DEBUGF(opt.NETIF_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_STATE, "netif: IP address of interface {0}{1} set to {2}.{3}.{4}.{5}\n",
				name[0], name[1],
				ip_addr.ip4_addr1_16(this.ip_addr),
				ip_addr.ip4_addr2_16(this.ip_addr),
				ip_addr.ip4_addr3_16(this.ip_addr),
				ip_addr.ip4_addr4_16(this.ip_addr));
		}

		public void set_gw(ip_addr gw)
		{
			ip_addr.ip_addr_set(this.gw, gw);
			lwip.LWIP_DEBUGF(opt.NETIF_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_STATE, "netif: GW address of interface {0}{1} set to {2}.{3}.{4}.{5}\n",
				name[0], name[1],
				ip_addr.ip4_addr1_16(this.gw),
				ip_addr.ip4_addr2_16(this.gw),
				ip_addr.ip4_addr3_16(this.gw),
				ip_addr.ip4_addr4_16(this.gw));
		}

		public void set_netmask(ip_addr netmask)
		{
			ip_addr.ip_addr_set(this.netmask, netmask);
			lwip.LWIP_DEBUGF(opt.NETIF_DEBUG | lwip.LWIP_DBG_TRACE | lwip.LWIP_DBG_STATE, "netif: netmask of interface {0}{1} set to {2}.{3}.{4}.{5}\n",
				name[0], name[1],
				ip_addr.ip4_addr1_16(this.netmask),
				ip_addr.ip4_addr2_16(this.netmask),
				ip_addr.ip4_addr3_16(this.netmask),
				ip_addr.ip4_addr4_16(this.netmask));
		}

		internal void NETIF_SET_HWADDRHINT(netif netif, object hint)
		{
			netif.addr_hint = hint;
		}

		internal static void netif_input(netif netif, byte[] packet)
		{
			throw new NotImplementedException();
		}

		internal static err_t output(netif netif, pbuf p, ip_addr src, ip_addr dest, byte proto)
		{
			int pos = 0, rest = p.tot_len;
			byte[] packet = new byte[rest];
			ip_addr srch = new ip_addr(lwip.lwip_ntohl(src.addr));
			ip_addr desth = new ip_addr(lwip.lwip_ntohl(dest.addr));

			for (pbuf q = p; q != null; q = q.next)
			{
				int len = rest;
				if (len > q.len)
					len = q.len;

				Buffer.BlockCopy(q.payload.data, q.payload.offset, packet, pos, len);
				pos += len;
				rest -= len;
			}

			netif.m_output(netif, packet, srch, desth, proto);

			return err_t.ERR_OK;
		}

		internal void input(byte[] packet, ip_addr srcn, ip_addr destn, byte proto)
		{
			ip_addr src = new ip_addr(lwip.lwip_htonl(srcn.addr));
			ip_addr dest = new ip_addr(lwip.lwip_htonl(destn.addr));
			pbuf p = lwip.pbuf_alloc(pbuf_layer.PBUF_RAW, (ushort)packet.Length, pbuf_type.PBUF_POOL);
			int pos = 0, rest = packet.Length;

			for (pbuf q = p; q != null; q = q.next)
			{
				int len = rest;
				if (len > q.len)
					len = q.len;

				Buffer.BlockCopy(packet, pos, q.payload.data, q.payload.offset, len);
				pos += len;
				rest -= len;
			}

			if (lwip.ip.ip_input(p, src, dest, proto, this) != err_t.ERR_OK)
				lwip.pbuf_free(p);
		}
	}

	partial class lwip
	{
		internal netif netif;
	}

	public class eth_addr
	{
		private byte v1;
		private byte v2;
		private byte v3;
		private byte v4;
		private byte v5;
		private byte v6;

		public eth_addr(byte v1, byte v2, byte v3, byte v4, byte v5, byte v6)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
			this.v5 = v5;
			this.v6 = v6;
		}
	}
}
