using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct T_CMPL
	{
		public object exinf;
		public pointer addr;
		public int mplsz;
	}

	public struct T_RMPL
	{
		public object exinf;
		public bool wtsk;
		public int frsz;
		public int maxsz;
	}

	internal class MemoryPool
	{
		ID m_MplID;
		T_CMPL m_cmpl;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
#if DEBUG
		_CrtMemBlockHeader m_pFirstBlock;
		_CrtMemBlockHeader m_pLastBlock;
		int m_nBlockCount;
		bool m_Dumped;
#endif
		TMemNodeList m_FreeMem = new TMemNodeList();
		Nucleus m_Nucleus;
		int m_FreeSize;
		int m_MaxSize;

		public MemoryPool(ID mplid, ref T_CMPL pk_cmpl, Nucleus pNucleus)
		{
			TMemNode Block = new TMemNode(pk_cmpl.addr, pk_cmpl.mplsz);

			m_MplID = mplid;
			m_cmpl = pk_cmpl;
			m_Nucleus = pNucleus;
			m_MaxSize = pk_cmpl.mplsz;
			m_FreeSize = pk_cmpl.mplsz;

			m_FreeMem.AddLast(Block);
#if DEBUG
			m_pFirstBlock = null;
			m_pLastBlock = null;
			m_nBlockCount = 0;
			m_Dumped = false;
#endif
		}

		public ID MplID { get { return m_MplID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CMPL cmpl { get { return m_cmpl; } }

		public ER ReferStatus(ref T_RMPL pk_rmpl)
		{
			//if (pk_rmpl == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rmpl.exinf = m_cmpl.exinf;

			// 待ちタスクの有無
			pk_rmpl.wtsk = m_TskQueue.First != null;

			// 獲得可能なメモリ・ブロックの合計サイズ
			pk_rmpl.frsz = m_FreeSize;

			// 獲得可能なメモリ・ブロックの最大サイズ
			pk_rmpl.maxsz = m_MaxSize;

			return ER.E_OK;
		}

		private void ChangeAddress(ref TMemNode Target, TMemNode New, int Size)
		{
			New.Previous = Target.Previous;
			New.Next = Target.Next;
			New.Size = Size;

			if (New.Previous != null) {
				System.Diagnostics.Debug.Assert(New.Previous.Next == Target);

				New.Previous.Next = New;
			}

			if (New.Next != null) {
				System.Diagnostics.Debug.Assert(New.Next.Previous == Target);

				New.Next.Previous = New;
			}

			if (m_FreeMem.First == Target) {
				m_FreeMem.First = New;
			}

			if (m_FreeMem.Last == Target) {
				m_FreeMem.Last = New;
			}

			Target = New;
		}

		private TMemNode Allocate(TMemNode Node, int Size)
		{
			TMemNode FreeMem = Node;
			TMemNode Result;

			// 取得サイズが空き領域に対してある程度小さい場合
			if (Size < FreeMem.Size / 8) {
				// 空き領域の先頭から割り当て
				Result = FreeMem;
				ChangeAddress(ref FreeMem, new TMemNode(new pointer(FreeMem, Size), FreeMem.Size - Size),
					FreeMem.Size - Size);
				Result = new TMemNode(Result, Size);
			}
			// 大きい場合
			else {
				// 空き領域の末尾から割り当て
				FreeMem.Size = FreeMem.Size - Size;
				Result = new TMemNode(new pointer(FreeMem, FreeMem.Size), Size);
			}

			return Result;
		}

		private TMemNode Allocate(int Size)
		{
			TMemNode Node, Temp;
			int Pad;

			// サイズ制限
			if (Size < TMemNode.GetMinimumSize())
				Size = TMemNode.GetMinimumSize();

			Size += TMemNode.GetHeaderSize() + TMemNode.GetFooterSize();

			// アライメント調整
			Pad = Size & 0x3;
			if (Pad != 0) {
				Size += 4 - Pad;
			}

			// 取得サイズが空き領域に対してある程度小さい場合
			if (Size < m_cmpl.mplsz / 8) {
				// 空き領域リストの先頭から検索
				Temp = m_FreeMem.First;
				while (Temp != null) {
					Node = Temp;
					Temp = Node.Next;

					// 取得サイズより大きい空き領域の場合
					if (Size < Node.Size) {
						// 空き領域からメモリ取得
						return Allocate(Node, Size);
					}
					// 取得サイズと等しい空き領域の場合
					else if (Size == Node.Size) {
						m_FreeMem.Remove(Node);
						return new TMemNode(Node, Size);
					}
				}
			}
			// 大きい場合
			else {
				// 空き領域リストの末尾から検索
				Temp = m_FreeMem.Last;
				while (Temp != null) {
					Node = Temp;
					Temp = Node.Previous;

					// 取得サイズより大きい空き領域の場合
					if (Size < Node.Size) {
						// 空き領域からメモリ取得
						return Allocate(Node, Size);
					}
					// 取得サイズと等しい空き領域の場合
					else if (Size == Node.Size) {
						m_FreeMem.Remove(Node);
						return new TMemNode(Node, Size);
					}
				}
			}
#if DEBUG
			if (!m_Dumped) {
				m_Nucleus.EnumMemoryBlock(this);
				m_Dumped = true;
			}
#endif
			return null;
		}

		private bool Free(TMemNode Mem)
		{
			TMemNode Node;
			TMemNode FreeMem;
			pointer NextAddress = ((pointer)Mem) + Mem.Size;
			int FreeNextAddress;

			// 解放メモリサイズが空き領域に対してある程度小さい場合
			if (Mem.Size < m_cmpl.mplsz / 8) {
				// 空き領域リストの先頭から検索
				TMemNode Next;
				TMemNode NextFreeMem;

				// 解放メモリのアドレスが、管理領域の先頭より前の場合
				if (Mem < m_cmpl.addr) {
					return false;
				}

				Next = m_FreeMem.First;
				while (Next != null) {
					Node = Next;
					Next = Node.Next;
					FreeMem = Node;
					FreeNextAddress = FreeMem.offset + FreeMem.Size;

					// 解放メモリが空き領域より前にある場合
					if (NextAddress < FreeMem) {
						// 空き領域の前に解放メモリを空き領域として追加
						m_FreeMem.AddBefore(Node, Mem);

						return true;
					}
					// 解放メモリのアドレスが空き領域の次のアドレスの場合
					else if (Mem.offset == FreeNextAddress) {
						// 空き領域の後ろに解放メモリを融合
						FreeMem.Size += Mem.Size;

						if (Next != null) {
							// 次の空き領域のアドレスが解放メモリの次のアドレスの場合
							NextFreeMem = Next;
							if (NextAddress == ((pointer)NextFreeMem)) {
								// 空き領域に次の空き領域を融合
								FreeMem.Size += NextFreeMem.Size;

								// 次の空き領域を削除
								m_FreeMem.Remove(Next);
							}
						}

						return true;
					}
					// 空き領域の次のアドレスが解放メモリのアドレスの場合
					else if (NextAddress == FreeMem) {
						// 空き領域の前に解放メモリを融合
						ChangeAddress(ref FreeMem, Mem, FreeMem.Size + Mem.Size);

						return true;
					}
					// 解放メモリが不正の場合
					else if (((Mem > FreeMem) && (Mem.offset < FreeNextAddress))
						|| ((NextAddress > FreeMem) && (NextAddress.offset < FreeNextAddress))) {
						return false;
					}
				}

				// 解放メモリのアドレスが、管理領域内の場合
				if (NextAddress <= (m_cmpl.addr + m_cmpl.mplsz)) {
					// 空き領域リストの最後に解放メモリを空き領域として追加
					m_FreeMem.AddLast(Mem);

					return true;
				}
			}
			// 大きい場合
			else {
				// 空き領域リストの末尾から検索
				TMemNode Prev;

				// 解放メモリのアドレスが、管理領域の末尾より後の場合
				if (NextAddress > (m_cmpl.addr + m_cmpl.mplsz)) {
					return false;
				}

				Prev = m_FreeMem.Last;
				while (Prev != null) {
					Node = Prev;
					Prev = Node.Previous;
					FreeMem = Node;
					FreeNextAddress = FreeMem.offset + FreeMem.Size;

					// 解放メモリが空き領域より後ろにある場合
					if (Mem.offset > FreeNextAddress) {
						// 空き領域の後に解放メモリを空き領域として追加
						m_FreeMem.AddAfter(Node, Mem);

						return true;
					}
					// 解放メモリの次のアドレスが空き領域のアドレスの場合
					else if (NextAddress == FreeMem) {
						// 空き領域の前に解放メモリを融合
						ChangeAddress(ref FreeMem, new TMemNode(new pointer(FreeMem, -Mem.Size), FreeMem.Size + Mem.Size),
							FreeMem.Size + Mem.Size);

						if (Prev != null) {
							// 前の空き領域のアドレスが解放メモリの次のアドレスの場合
							Mem = Prev;
							if (FreeMem.offset == (Mem.offset + Mem.Size)) {
								// 空き領域の前に前の空き領域を融合
								ChangeAddress(ref FreeMem, new TMemNode(new pointer(FreeMem, -Mem.Size), FreeMem.Size + Mem.Size),
									FreeMem.Size + Mem.Size);

								// 前の空き領域を削除
								m_FreeMem.Remove(Prev);
							}
						}

						return true;
					}
					// 解放メモリのアドレスが空き領域の次のアドレスの場合
					else if (Mem.offset == FreeNextAddress) {
						// 空き領域リストの最後に解放メモリを空き領域として追加
						FreeMem.Size += Mem.Size;

						return true;
					}
					// 解放メモリが不正の場合
					else if (((Mem > FreeMem) && (Mem.offset < FreeNextAddress))
						|| ((NextAddress > FreeMem) && (NextAddress.offset < FreeNextAddress))) {
						return false;
					}
				}

				// 解放メモリのアドレスが、管理領域内の場合
				if (Mem >= m_cmpl.addr) {
					// 空き領域リストの最初に解放メモリを空き領域として追加
					m_FreeMem.AddFirst(Mem);

					return true;
				}
			}

			return false;
		}

		public void UpdateFreeSize()
		{
			int Max = 0;
			int Total = 0;

			for (TMemNode Node = m_FreeMem.First; Node != null; Node = Node.Next) {
				int Size = Node.Size;
				if (Max < Size)
					Max = Size;
				Total += Size;
			}

			m_MaxSize = Max;
			m_FreeSize = Total;
		}

		public ER GetMemoryBlock(out pointer p_blk, int blksz, TMO tmout)
		{
			ER ret;

			p_blk = null;

			//if (p_blk == null)
			//	return ER.E_PAR;

			for (; ; ) {
				TMemNode Node = Allocate(blksz);

				if (Node != null) {
					p_blk = Node.GetData();
					break;
				}

				if (tmout == 0)
					return ER.E_TMOUT;

				Task task = m_Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					return ER.E_CTX;

				task.MemoryBlockSize = blksz;
				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_MPL, m_MplID, tmout);

				switch (ret) {
				case ER.E_OK:
					continue;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}
#if DEBUG
			string lpszFileName = "";
			int nLine = 0;
			_CrtMemBlockHeader.AddBlock(m_pFirstBlock, m_pLastBlock, p_blk, blksz, lpszFileName, nLine);
			m_nBlockCount++;
#endif
			return ER.E_OK;
		}

		public ER ReleaseMemoryBlock(pointer blk)
		{
			if (blk == null)
				return ER.E_PAR;

			//if ((int)blk != 0)
			//	return ER.E_PAR;

			System.Diagnostics.Debug.Assert((blk >= m_cmpl.addr) && (blk < m_cmpl.addr + m_cmpl.mplsz));

#if DEBUG
			_CrtMemBlockHeader.DelBlock(m_pFirstBlock, m_pLastBlock, blk);
			m_nBlockCount--;
#endif
			TMemNode Mem = TMemNode.GetNode(blk);

			if (!Free(Mem))
				return ER.E_PAR;

			UpdateFreeSize();

#if DEBUG
			pointer.memset(Mem + 1, 0xDD, Mem.Size - TMemNode.length);
#endif
			for (LinkedListNode<Task> Node = m_TskQueue.First; Node != null; Node = Node.Next) {
				Task task = Node.Value;
				if (task.MemoryBlockSize <= m_MaxSize) {
					m_TskQueue.Remove(Node);

					if (!task.ReleaseWait())
						return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void EnumMemoryBlock(object Obj, TEnumMemoryBlockCallBack CallBack)
		{
#if DEBUG
			_CrtMemBlockHeader.EnumBlock(m_pFirstBlock, Obj, CallBack);
#endif
		}
	}
}
