using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uITron3
{
	//------------------------------------------------------------------------------
	// 贋割り込み情報
	//------------------------------------------------------------------------------
	public delegate ID TInterruptEvent(object self);

	//------------------------------------------------------------------------------
	// Driver⇔Simulator
	//------------------------------------------------------------------------------
	public interface ISystemIF
	{
		byte GetByte(uint addr);
		void SetByte(uint addr, byte value);
		ushort GetUInt16(uint addr);
		void SetUInt16(uint addr, ushort value);
		uint GetUInt32(uint addr);
		void SetUInt32(uint addr, uint value);
	}

	//-------------------------------------------------------------------------
	// TSystemIFItem
	//-------------------------------------------------------------------------
	public struct TSystemIFItem
	{
		public uint Addr;
		public int Size;
		public uint Substitute;
		public ISystemIF SystemIF;
	}

	//------------------------------------------------------------------------------
	// Driver⇔Simulator
	//------------------------------------------------------------------------------
	public interface ISysTimerSync
	{
		long GetTimer();
		void Progress(long interval);
		void CallTimeOut();
	}
}
