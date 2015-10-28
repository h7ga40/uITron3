using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	internal class TMemNodeList
	{
		public TMemNode First;
		public TMemNode Last;
		public int Count;

		public TMemNodeList()
		{
			First = null;
			Last = null;
			Count = 0;
		}

		public void AddFirst(TMemNode Item)
		{
			//キューにまだノードが一つも無い場合
			if (First == null) {
				//System.Diagnostics.Debug.Assert(Count == 0);

				//新しいItemの次のItemはなしに設定
				Item.Next = null;

				//新しいItemを最後のItemに設定
				Last = Item;
			}
			else {
				//最初のItemの前は新しいItemに設定
				First.Previous = Item;

				//新しいItemを最初のItemの次のItemに設定
				Item.Next = First;
			}

			//新しいItemを最初のItemに設定
			First = Item;

			//新しいItemの前のItemはなしに設定
			Item.Previous = null;

			Count++;
		}

		public void AddLast(TMemNode Item)
		{
			//キューにまだノードが一つも無い場合
			if (Last == null) {
				//System.Diagnostics.Debug.Assert(Count == 0);

				//新しいItemの前のItemはなしに設定
				Item.Previous = null;

				//新しいItemを最初のItemに設定
				First = Item;
			}
			else {
				//最後のItemの次は新しいItemに設定
				Last.Next = Item;

				//新しいItemを最後のItemの前のItemに設定
				Item.Previous = Last;
			}

			//新しいItemを最後のItemに設定
			Last = Item;

			//新しいItemの次のItemはなしに設定
			Item.Next = null;

			Count++;
		}

		public void AddBefore(TMemNode Pos, TMemNode Item)
		{
			//キューにまだノードが一つも無い場合
			if (First == null) {
				//System.Diagnostics.Debug.Assert(Count == 0);

				//新しいItemの次のItemはなしに設定
				Item.Next = null;

				//新しいItemを最後のItemに設定
				Last = Item;

				//新しいItemを最初のItemに設定
				First = Item;

				//新しいItemの前のItemはなしに設定
				Item.Previous = null;
			}
			else if (Pos == null) {
				//最初のItemの前は新しいItemに設定
				First.Previous = Item;

				//新しいItemを最初のItemの次のItemに設定
				Item.Next = First;

				//新しいItemを最初のItemに設定
				First = Item;

				//新しいItemの前のItemはなしに設定
				Item.Previous = null;
			}
			else {
				Item.Previous = Pos.Previous;
				Item.Next = Pos;

				if (Pos.Previous != null) {
					Pos.Previous.Next = Item;
				}
				else {
					First = Item;
				}
				Pos.Previous = Item;

				Count++;
			}
		}

		public void AddAfter(TMemNode Pos, TMemNode Item)
		{
			//キューにまだノードが一つも無い場合
			if (Last == null) {
				//System.Diagnostics.Debug.Assert(Count == 0);

				//新しいItemの前のItemはなしに設定
				Item.Previous = null;

				//新しいItemを最初のItemに設定
				First = Item;

				//新しいItemを最後のItemに設定
				Last = Item;

				//新しいItemの次のItemはなしに設定
				Item.Next = null;
			}
			else if (Pos == null) {
				//最後のItemの次は新しいItemに設定
				Last.Next = Item;

				//新しいItemを最後のItemの前のItemに設定
				Item.Previous = Last;

				//新しいItemを最後のItemに設定
				Last = Item;

				//新しいItemの次のItemはなしに設定
				Item.Next = null;
			}
			else {
				Item.Next = Pos.Next;
				Item.Previous = Pos;

				if (Pos.Next != null) {
					Pos.Next.Previous = Item;
				}
				else {
					Last = Item;
				}
				Pos.Next = Item;

				Count++;
			}
		}

		public void Remove(TMemNode Item)
		{
			if (First == Item) {
				First = Item.Next;
			}

			if (Last == Item) {
				Last = Item.Previous;
			}

			//削除するノードの関連付けを次のノードに設定
			if (Item.Previous != null) {
				Item.Previous.Next = Item.Next;
			}

			//削除するノードの関連付けを前のノードに設定
			if (Item.Next != null) {
				Item.Next.Previous = Item.Previous;
			}

			Count--;

			//削除したノードを返して終了
			Item.Previous = null;
			Item.Next = null;
		}
	}
}
