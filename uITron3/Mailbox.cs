using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class T_MSG : pointer
	{
		public struct Fields
		{
			public static readonly pointer_field_info msgrfu = new pointer_field_info(0);
		}
		public new const int length = 4;

		public T_MSG(byte[] blk, int offset) : base(blk, offset) { }
		public T_MSG(pointer blk) : base(blk) { }
		public T_MSG(pointer blk, int offset) : base(blk, offset) { }

		//public pointer msgrfu { get { return GetFieldValue(Fields.msgrfu); } set { SetFieldValue(Fields.msgrfu, value); } }
		public object msgrfu;
	}

	public struct T_CMBX
	{
		public object exinf;
	}

	public struct T_RMBX
	{
		public object exinf;
		public bool wtsk;
		public T_MSG pk_msg;
	}

	internal class Mailbox
	{
		ID m_MbxID;
		T_CMBX m_cmbx;
		LinkedList<Task> m_TskQueue = new LinkedList<Task>();
		LinkedList<T_MSG> m_MsgQueue = new LinkedList<T_MSG>();
		Nucleus m_Nucleus;

		public Mailbox(ID mbxid, ref T_CMBX pk_cmbx, Nucleus pNucleus)
		{
			m_MbxID = mbxid;
			m_cmbx = pk_cmbx;
			m_Nucleus = pNucleus;
		}

		public ID MbxID { get { return m_MbxID; } }

		public Nucleus Nucleus { get { return m_Nucleus; } }

		public T_CMBX cmbx { get { return m_cmbx; } }

		public ER ReferStatus(ref T_RMBX pk_rmbx)
		{
			//if (pk_rmbx == null)
			//	return ER.E_PAR;

			// 拡張情報
			pk_rmbx.exinf = m_cmbx.exinf;

			// 待ちタスクの有無
			pk_rmbx.wtsk = m_TskQueue.First != null;

			// 待ちメッセージの有無
			pk_rmbx.pk_msg = (m_MsgQueue.First == null) ? null : m_MsgQueue.First.Value;

			return ER.E_OK;
		}

		public ER SendMessage(ref T_MSG pk_msg)
		{
			if (pk_msg == null)
				return ER.E_PAR;

			if (pk_msg.msgrfu != null)
				return ER.E_OBJ;

			m_MsgQueue.AddLast(pk_msg);

			if (m_TskQueue.First != null) {
				Task task = m_TskQueue.First.Value;
				m_TskQueue.RemoveFirst();

				if (!task.ReleaseWait())
					return ER.E_RLWAI;
			}

			return ER.E_OK;
		}

		public ER ReceiveMessage(out T_MSG ppk_msg, TMO tmout)
		{
			ER ret;

			ppk_msg = null;

			Task task = m_Nucleus.GetTask(ID.TSK_SELF);

			if ((tmout != 0) && (task == null))
				return ER.E_CTX;

			//if (ppk_msg == null)
			//	return ER.E_PAR;

			if ((m_TskQueue.First == null) && (m_MsgQueue.First != null)) {
				ppk_msg = m_MsgQueue.First.Value;
				m_MsgQueue.RemoveFirst();
			}
			else {
				if (task == null)
					return ER.E_TMOUT;

				if (tmout == 0)
					return ER.E_TMOUT;

				ret = task.Wait(m_TskQueue, TSKWAIT.TTW_MBX, m_MbxID, tmout);

				switch (ret) {
				case ER.E_OK:
					if (m_MsgQueue.First == null)
						return ER.E_RLWAI;
					ppk_msg = m_MsgQueue.First.Value;
					m_MsgQueue.RemoveFirst();
					break;
				case ER.E_TMOUT:
					return ER.E_TMOUT;
				default:
					return ER.E_RLWAI;
				}
			}

			//SetDebugInfo(ppk_msg, lpszFileName, nLine);

			return ER.E_OK;
		}
	}
}
