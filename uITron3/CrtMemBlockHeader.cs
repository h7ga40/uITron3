using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	internal delegate bool TEnumMemoryBlockCallBack(object Obj, _CrtMemBlockHeader MemBlock);

	internal class _CrtMemBlockHeader : pointer
	{
		public struct Fields
		{
			public static readonly value_field_info<uint> gap = new value_field_info<uint>(0);
		}
		public new const int length = 2 * nNoMansLandSize;

		public const int _FREE_BLOCK = 0;
		public const int _NORMAL_BLOCK = 1;
		public const int _CRT_BLOCK = 2;
		public const int _IGNORE_BLOCK = 3;
		public const int _CLIENT_BLOCK = 4;
		public const int _MAX_BLOCKS = 5;

		public const int nNoMansLandSize = 4;

		public _CrtMemBlockHeader pBlockHeaderNext;
		public _CrtMemBlockHeader pBlockHeaderPrev;
		public string szFileName;
		public int nLine;
		public int nDataSize;
		public int nBlockUse;
		public int lRequest;
		public uint gap { get { return Fields.gap.get(this); } }

		public _CrtMemBlockHeader(pointer src, int offset,
			_CrtMemBlockHeader pBlockHeaderNext, _CrtMemBlockHeader pBlockHeaderPrev,
			string szFileName, int nLine, int nDataSize, int nBlockUse, int lRequest)
			: base(src, offset)
		{
			this.pBlockHeaderNext = pBlockHeaderNext;
			this.pBlockHeaderPrev = pBlockHeaderPrev;
			this.szFileName = szFileName;
			this.nLine = nLine;
			this.nDataSize = nDataSize;
			this.nBlockUse = nBlockUse;
			this.lRequest = lRequest;
		}

		internal static void AddBlock(_CrtMemBlockHeader _pFirstBlock, _CrtMemBlockHeader _pLastBlock, pointer p_blk, int blksz, string lpszFileName, int nLine)
		{
		}

		internal static void DelBlock(_CrtMemBlockHeader _pFirstBlock, _CrtMemBlockHeader _pLastBlock, pointer blk)
		{
		}

		internal static void EnumBlock(_CrtMemBlockHeader _pFirstBlock, object Obj, TEnumMemoryBlockCallBack CallBack)
		{
			for (_CrtMemBlockHeader Pos = _pFirstBlock;
				Pos != null;
				Pos = Pos.pBlockHeaderNext) {
				if (CallBack(Obj, Pos))
					break;
			}
		}
	}
}
