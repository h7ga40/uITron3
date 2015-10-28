using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class TMemNode : pointer
	{
		public struct Fields
		{
			public static readonly value_field_info<int> Size = new value_field_info<int>(0);
			internal static readonly struct_field_info<_CrtMemBlockHeader> m_Header = new struct_field_info<_CrtMemBlockHeader>(4);
			public static readonly pointer_field_info<TMemNode> Previous = new pointer_field_info<TMemNode>(4 + _CrtMemBlockHeader.length);
		}
		public int Size;
#if DEBUG
		internal _CrtMemBlockHeader m_Header;
#endif
		public TMemNode Previous;
		public TMemNode Next;
#if DEBUG
		public byte[] gap = new byte[_CrtMemBlockHeader.nNoMansLandSize];
#endif
		public TMemNode(pointer data, int size)
			: base(data)
		{
#if DEBUG
			m_Header = new _CrtMemBlockHeader(this, 0, null, null, "", 0, size,
				_CrtMemBlockHeader._NORMAL_BLOCK, 0);
#endif
			Previous = null;
			Next = null;
			Size = size;
		}

		public pointer GetData() { return new pointer(this, Fields.Previous.offset); }

		public static TMemNode GetNode(pointer Data)
		{
			return new TMemNode(Data, -Fields.Previous.offset);
		}

		public static int GetHeaderSize()
		{
			return Fields.Previous.offset;
		}

		public static int GetFooterSize()
		{
#if DEBUG
			return _CrtMemBlockHeader.nNoMansLandSize;
#else
			return 0;
#endif
		}

		public static int GetMinimumSize()
		{
			return TMemNode.length - Fields.Previous.offset;
		}
	}
}
