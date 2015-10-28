using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public delegate ER PacketBridgeInputData(byte[] data);

	public class PacketBridge
	{
		Kernel m_Kernel;
		int m_Kind;
		PacketBridgeInputData m_InputData;

		public PacketBridge(Kernel pKernel, int kind)
		{
			m_Kernel = pKernel;
			m_Kind = kind;
		}

		public void OutputData(byte[] data)
		{
			m_Kernel.Output(m_Kind, data, data.Length);
		}

		public int IFKind { get { return m_Kind; } }

		public PacketBridgeInputData InputData
		{
			get { return m_InputData; }
			set { m_InputData = value; }
		}
	}
}
