using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public struct T_CMPF
	{
		public object exinf;
		public pointer addr;
		public int blkcnt;
		public int blksz;
	}

	public struct T_RMPF
	{
		public object exinf;
		public bool wtsk;
		public int fblkcnt;
	}

	internal class MemoryPoolFixedsize
	{
		ID m_MpfID;
		T_CMPF m_cmpf;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		LinkedList<pointer> m_MpfQueue = new LinkedList<pointer>();
		Nucleus m_Nucleus;

		public MemoryPoolFixedsize(ID mpfid, ref T_CMPF pk_cmpf, Nucleus pNucleus)
		{
			pointer Block;

			m_MpfID = mpfid;
			m_cmpf = pk_cmpf;
			m_Nucleus = pNucleus;

			for (int i = 0; i < pk_cmpf.blkcnt; i++) {
				Block = new pointer(pk_cmpf.addr, i * pk_cmpf.blksz);
				pointer.memset(Block, 0, pk_cmpf.blksz);
				m_MpfQueue.AddLast(Block);
			}
		}

		public ID MpfID { get { return m_MpfID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CMPF cmpf { get { return m_cmpf; } }

		public ER ReferStatus(ref T_RMPF pk_rmpf)
		{
			//if (pk_rmpf == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rmpf.exinf = m_cmpf.exinf;

			// 待ちタスクの有無
			pk_rmpf.wtsk = m_TskQueue.First != null;

			// 獲得可能なメモリ・ブロックの数
			pk_rmpf.fblkcnt = m_MpfQueue.Count;

			return ER.E_OK;
		}

		public ER GetMemoryBlock(out pointer p_blk, int blksz, TMO tmout)
		{
			ER ret;

			p_blk = null;

			//if (p_blk == null)
			//	return ER.E_PAR;

			for (; ; ) {
				LinkedListNode<pointer> Node = m_MpfQueue.First;

				if (Node != null) {
					p_blk = Node.Value;
					m_MpfQueue.RemoveFirst();
					break;
				}

				if (tmout == 0)
					return ER.E_TMOUT;

				Task task = m_Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					return ER.E_CTX;

				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_MPF, m_MpfID, tmout);

				switch (ret) {
				case ER.E_OK:
					continue;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			return ER.E_OK;
		}

		public ER ReleaseMemoryBlock(pointer blk)
		{
			if (blk == null)
				return ER.E_PAR;

			//if ((int)blk != 0)
			//	return ER.E_PAR;

			if ((blk < m_cmpf.addr) || (blk >= m_cmpf.addr + (m_cmpf.blkcnt * m_cmpf.blksz)))
				return ER.E_PAR;

			m_MpfQueue.AddLast(blk);

			for (LinkedListNode<Task> Node = m_TskQueue.First; Node != null; Node = Node.Next) {
				Task task = Node.Value;

				m_TskQueue.Remove(Node);

				if (!task.ReleaseWait())
					return ER.E_RLWAI;
			}

			return ER.E_OK;
		}
	}
}
