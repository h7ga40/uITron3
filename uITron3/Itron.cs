using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uITron3
{
	public enum ER
	{
		/// <summary>正常終了</summary>
		E_OK = 0,
		/// <summary>システムエラー</summary>
		E_SYS = -5,
		/// <summary>未サポート機能</summary>
		E_NOSPT = -9,
		/// <summary>予約機能コード</summary>
		E_RSFN = -10,
		/// <summary>予約属性</summary>
		E_RSATR = -11,
		/// <summary>パラメータエラー</summary>
		E_PAR = -17,
		/// <summary>不正ID番号</summary>
		E_ID = -18,
		/// <summary>コンテキストエラー</summary>
		E_CTX = -25,
		/// <summary>メモリアクセス違反</summary>
		E_MACV = -26,
		/// <summary>オブジェクトアクセス違反</summary>
		E_OACV = -27,
		/// <summary>サービスコール不正使用</summary>
		E_ILUSE = -28,
		/// <summary>メモリ不足</summary>
		E_NOMEM = -33,
		/// <summary>ID番号不足</summary>
		E_NOID = -34,
		/// <summary>資源不足</summary>
		E_NORES = -35,
		/// <summary>オブジェクト状態エラー</summary>
		E_OBJ = -41,
		/// <summary>オブジェクト未登録</summary>
		E_NOEXS = -42,
		/// <summary>キューイングオーバフロー</summary>
		E_QOVR = -43,
		/// <summary>待ち禁止状態または待ち状態の強制解除</summary>
		E_RLWAI = -49,
		/// <summary>ポーリング失敗またはタイムアウト</summary>
		E_TMOUT = -50,
		/// <summary>待ちオブジェクトの削除または再初期化</summary>
		E_DLT = -51,
		/// <summary>待ちオブジェクトの状態変化</summary>
		E_CLS = -52,
		/// <summary>ノンブロッキング受付け</summary>
		E_WBLK = -57,
		/// <summary>バッファオーバフロー</summary>
		E_BOVR = -58,
	}

	public struct ID
	{
		public static ID TSK_SELF = new ID(0);
		public static ID TSK_NULL = new ID(0);
		public static ID ID_AUTO = new ID(-1);
		public static ID NULL = new ID(0);

		public int Value;

		public ID(int value)
		{
			Value = value;
		}

		public static implicit operator int (ID a)
		{
			return a.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct ATR
	{
		/// <summary>オブジェクト属性を指定しない</summary>
		public static ATR TA_NULL = new ATR(0);
		/// <summary>高級言語用インタフェース</summary>
		public static ATR TA_HLNG = new ATR(0x00);
		/// <summary>タスクの待ち行列をFIFO順に</summary>
		public static ATR TA_TFIFO = new ATR(0x00);
		/// <summary>メッセージキューをFIFO順に</summary>
		public static ATR TA_MFIFO = new ATR(0x00);
		/// <summary>待ちタスクは1つのみ</summary>
		public static ATR TA_WSGL = new ATR(0x00);
		/// <summary> タスクを起動された状態で生成 </summary>
		public static ATR TA_ACT = new ATR(0x02);
		/// <summary> タスクの待ち行列を優先度順に </summary>
		public static ATR TA_TPRI = new ATR(0x01);
		/// <summary> メッセージキューを優先度順に </summary>
		public static ATR TA_MPRI = new ATR(0x02);
		/// <summary> 複数の待ちタスク </summary>
		public static ATR TA_WMUL = new ATR(0x02);
		/// <summary> イベントフラグのクリア指定 </summary>
		public static ATR TA_CLR = new ATR(0x04);
		/// <summary> 周期ハンドラを動作状態で生成 </summary>
		public static ATR TA_STA = new ATR(0x02);
		/// <summary> カーネル管理外の割込み </summary>
		public static ATR TA_NONKERNEL = new ATR(0x02);
		/// <summary> 割込み要求禁止フラグをクリア </summary>
		public static ATR TA_ENAINT = new ATR(0x01);
		/// <summary> エッジトリガ </summary>
		public static ATR TA_EDGE = new ATR(0x02);

		public int Value;

		public ATR(int value)
		{
			Value = value;
		}

		public static implicit operator int (ATR a)
		{
			return a.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct TMO
	{
		/// <summary>ポーリング</summary>
		public static TMO TMO_POL = new TMO(0);
		/// <summary>永久待ち</summary>
		public static TMO TMO_FEVR = new TMO(-1);
		/// <summary>ノンブロッキングコール</summary>
		public static TMO TMO_NBLK = new TMO(-2);

		public long Value;

		public TMO(long value)
		{
			Value = value;
		}

		public static implicit operator long (TMO a)
		{
			return a.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct HNO
	{
		public int Value;

		public HNO(int value)
		{
			Value = value;
		}

		public static implicit operator int (HNO a)
		{
			return a.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct PRI
	{
		public static PRI TPRI_INI = new PRI(0);

		public int Value;

		public PRI(int value)
		{
			Value = value;
		}

		public static implicit operator int (PRI a)
		{
			return a.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public struct T_DINT
	{
		public TInterruptEvent inthdr;
		public object gp;
	}

	public enum SYSSTAT
	{
		TTS_TSK = 0,
		TTS_DDSP = 1,
		TTS_LOC = 3,
		TTS_INDP = 4,
		TTS_QTSK = 8,
	}

	public struct T_RSYS
	{
		public SYSSTAT sysstat;
	}

	public struct T_VER
	{
	}

	public struct SYSTIME
	{
		public long Value;
	}

	public struct DLYTIME
	{
	}

	public class Itron
	{
		public static int TASK_PRI_NUM = 8;
		public static T_DCYC NADR = new T_DCYC();

		private Kernel g_Kernel;

		public Itron(Kernel kernel)
		{
			g_Kernel = kernel;
		}

		public void Init()
		{
			g_Kernel.Init(this);
		}

		public Kernel Kernel { get { return g_Kernel; } }

		private ushort SIL_REV_ENDIAN_H(ushort data)
		{
			return (ushort)(((data & 0xff) << 8) | ((data >> 8) & 0xff));
		}

		private uint SIL_REV_ENDIAN_W(uint data)
		{
			return (uint)(((data & 0x00ff) << 24) | ((data & 0xff00) << 8)
				| ((data >> 8) & 0xff00) | ((data >> 24) & 0x00ff));
		}

		public byte sil_reb_mem(pointer mem)
		{
			return g_Kernel.GetSubByte((uint)mem);
		}

		public void sil_wrb_mem(pointer mem, byte data)
		{
			g_Kernel.SetSubByte((uint)mem, data);
		}

		public ushort sil_reh_mem(pointer mem)
		{
			return g_Kernel.GetSubUInt16((uint)mem);
		}

		public void sil_wrh_mem(pointer mem, ushort data)
		{
			g_Kernel.SetSubUInt16((uint)mem, data);
		}

		public ushort sil_reh_lem(pointer mem)
		{
			ushort data;

			data = g_Kernel.GetSubUInt16((uint)mem);

			return (SIL_REV_ENDIAN_H(data));
		}

		public void sil_wrh_lem(pointer mem, ushort data)
		{
			sil_wrh_mem(mem, SIL_REV_ENDIAN_H(data));
		}

		public ushort sil_reh_bem(pointer mem)
		{
			ushort data;

			data = g_Kernel.GetSubUInt16((uint)mem);

			return (SIL_REV_ENDIAN_H(data));
		}

		public void sil_wrh_bem(pointer mem, ushort data)
		{
			sil_wrh_mem(mem, SIL_REV_ENDIAN_H(data));
		}

		public uint sil_rew_mem(pointer mem)
		{
			return g_Kernel.GetSubUInt32((uint)mem);
		}

		public void sil_wrw_mem(pointer mem, uint data)
		{
			g_Kernel.SetSubUInt32((uint)mem, data);
		}

		public uint sil_rew_lem(pointer mem)
		{
			uint data;

			data = g_Kernel.GetSubUInt32((uint)mem);

			return (SIL_REV_ENDIAN_W(data));
		}

		public void sil_wrw_lem(pointer mem, uint data)
		{
			g_Kernel.SetSubUInt32((uint)mem, SIL_REV_ENDIAN_W(data));
		}

		public uint sil_rew_bem(pointer mem)
		{
			uint data;

			data = g_Kernel.GetSubUInt32((uint)mem);

			return (SIL_REV_ENDIAN_W(data));
		}

		public void sil_wrw_bem(pointer mem, uint data)
		{
			g_Kernel.SetSubUInt32((uint)mem, SIL_REV_ENDIAN_W(data));
		}

		public ER loc_cpu()
		{
			if (g_Kernel == null) {
				return ER.E_DLT;
			}

			g_Kernel.LockCPU();

			return ER.E_OK;
		}

		public ER unl_cpu()
		{
			if (g_Kernel == null) {
				return ER.E_DLT;
			}

			g_Kernel.UnlockCPU();

			return ER.E_OK;
		}

		public ER ena_int()
		{
			g_Kernel.UnlockCPU();

			return ER.E_OK;
		}

		public ER dis_int()
		{
			g_Kernel.LockCPU();

			return ER.E_OK;
		}

		public ER ena_dsp()
		{
			return ER.E_NOSPT;
		}

		public ER dis_dsp()
		{
			return ER.E_NOSPT;
		}

		public void ext_tsk()
		{
			Task task;

			if (g_Kernel == null)
				return;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task != null) {
					task.Exit();
				}
			}
			finally {
				g_Kernel.UnlockCPU();
			}
		}

		public void exd_tsk()
		{
			Task task;

			if (g_Kernel == null)
				return;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task != null) {
					task.Exit();
				}
			}
			finally {
				g_Kernel.UnlockCPU();
			}
		}

		public ER get_tid(ref ID p_tskid)
		{
			Task task;

			if (g_Kernel == null)
				return ER.E_DLT;

			if (p_tskid == 0)
				return ER.E_PAR;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				p_tskid = (task != null) ? task.TaskID : new ID(0);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return ER.E_OK;
		}

		public ER slp_tsk()
		{
			ER result = ER.E_NOEXS;
			Task task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					result = ER.E_CTX;
				else
					result = task.Sleep(TMO.TMO_FEVR);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return result;
		}

		public ER tslp_tsk(TMO tmout)
		{
			ER result = ER.E_NOEXS;
			Task task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					result = ER.E_CTX;
				else
					result = task.Sleep(tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return result;
		}

		public ER dly_tsk(DLYTIME dlytim)
		{
			ER result = ER.E_NOEXS;
			Task task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					result = ER.E_CTX;
				else
					result = task.Delay(dlytim);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return result;
		}

		public ER rot_rdq(PRI tskpri)
		{
			ER result = ER.E_NOEXS;
			Task task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				task = g_Kernel.Nucleus.GetTask(ID.TSK_SELF);

				if (task == null)
					result = ER.E_CTX;
				else
					result = task.RotateReadyQueue(tskpri);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return result;
		}

		public ER act_cyc(HNO cycno, CYCACT cycact)
		{
			CyclicHandler Cyc = null;

			ER ret = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				ret = g_Kernel.Nucleus.GetCyclicHandler(cycno, ref Cyc);
				if (ret == ER.E_OK)
					ret = Cyc.Activate(cycact);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return ret;
		}

		public ER ref_cyc(ref T_RCYC pk_rcyc, HNO cycno)
		{
			CyclicHandler Cyc = null;
			ER ret = ER.E_NOEXS;

			ret = g_Kernel.Nucleus.GetCyclicHandler(cycno, ref Cyc);
			if (ret != ER.E_OK)
				return ret;

			return Cyc.ReferStatus(ref pk_rcyc);
		}

		public ER get_ver(ref T_VER pk_ver)
		{
			return ER.E_NOSPT;
		}

		public ER ref_icr(byte[] p_regptn, uint dintno)
		{
			return ER.E_NOSPT;
		}

		public ER cre_mpl(ID mplid, ref T_CMPL pk_cmpl, out ID p_mplid)
		{
			ER Result = ER.E_NOEXS;

			p_mplid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateMemoryPool(mplid, ref pk_cmpl, out p_mplid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_mpl(ID mplid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteMemoryPool(mplid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER get_blk(out pointer p_blk, ID mplid, int blksz)
		{
			ER Result = ER.E_NOEXS;
			MemoryPool MemoryPool;

			p_blk = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPool = g_Kernel.Nucleus.GetMemoryPool(mplid);
				if (MemoryPool == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPool.GetMemoryBlock(out p_blk, blksz, TMO.TMO_FEVR);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER pget_blk(out pointer p_blk, ID mplid, int blksz)
		{
			ER Result = ER.E_NOEXS;
			MemoryPool MemoryPool;

			p_blk = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPool = g_Kernel.Nucleus.GetMemoryPool(mplid);
				if (MemoryPool == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPool.GetMemoryBlock(out p_blk, blksz, TMO.TMO_POL);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tget_blk(out pointer p_blk, ID mplid, int blksz, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			MemoryPool MemoryPool;

			p_blk = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPool = g_Kernel.Nucleus.GetMemoryPool(mplid);
				if (MemoryPool == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPool.GetMemoryBlock(out p_blk, blksz, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER rel_blk(ID mplid, pointer blk)
		{
			ER Result = ER.E_NOEXS;
			MemoryPool MemoryPool;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPool = g_Kernel.Nucleus.GetMemoryPool(mplid);
				if (MemoryPool == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPool.ReleaseMemoryBlock(blk);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_mpl(ref T_RMPL pk_rmpl, ID mplid)
		{
			ER Result = ER.E_NOEXS;
			MemoryPool MemoryPool;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPool = g_Kernel.Nucleus.GetMemoryPool(mplid);
				if (MemoryPool == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPool.ReferStatus(ref pk_rmpl);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER cre_mpf(ID mpfid, ref T_CMPF pk_cmpf, out ID p_mpfid)
		{
			ER Result = ER.E_NOEXS;

			p_mpfid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateMemoryPoolFixedsize(mpfid, ref pk_cmpf, out p_mpfid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_mpf(ID mpfid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteMemoryPoolFixedsize(mpfid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER get_blf(out pointer p_blf, ID mpfid, int blfsz)
		{
			ER Result = ER.E_NOEXS;
			MemoryPoolFixedsize MemoryPoolFixedsize;

			p_blf = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPoolFixedsize = g_Kernel.Nucleus.GetMemoryPoolFixedsize(mpfid);
				if (MemoryPoolFixedsize == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPoolFixedsize.GetMemoryBlock(out p_blf, blfsz, TMO.TMO_FEVR);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER pget_blf(out pointer p_blf, ID mpfid, int blfsz)
		{
			ER Result = ER.E_NOEXS;
			MemoryPoolFixedsize MemoryPoolFixedsize;

			p_blf = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPoolFixedsize = g_Kernel.Nucleus.GetMemoryPoolFixedsize(mpfid);
				if (MemoryPoolFixedsize == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPoolFixedsize.GetMemoryBlock(out p_blf, blfsz, TMO.TMO_POL);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tget_blf(out pointer p_blf, ID mpfid, int blfsz, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			MemoryPoolFixedsize MemoryPoolFixedsize;

			p_blf = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPoolFixedsize = g_Kernel.Nucleus.GetMemoryPoolFixedsize(mpfid);
				if (MemoryPoolFixedsize == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPoolFixedsize.GetMemoryBlock(out p_blf, blfsz, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER rel_blf(ID mpfid, pointer blf)
		{
			ER Result = ER.E_NOEXS;
			MemoryPoolFixedsize MemoryPoolFixedsize;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPoolFixedsize = g_Kernel.Nucleus.GetMemoryPoolFixedsize(mpfid);
				if (MemoryPoolFixedsize == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPoolFixedsize.ReleaseMemoryBlock(blf);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_mpf(ref T_RMPF pk_rmpf, ID mpfid)
		{
			ER Result = ER.E_NOEXS;
			MemoryPoolFixedsize MemoryPoolFixedsize;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				MemoryPoolFixedsize = g_Kernel.Nucleus.GetMemoryPoolFixedsize(mpfid);
				if (MemoryPoolFixedsize == null)
					Result = ER.E_NOEXS;
				else
					Result = MemoryPoolFixedsize.ReferStatus(ref pk_rmpf);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER cre_tsk(ID tskid, ref T_CTSK pk_ctsk, out ID p_takid)
		{
			ER Result = ER.E_NOEXS;

			p_takid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateTask(tskid, ref pk_ctsk, out p_takid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteTask(tskid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER sta_tsk(ID tskid, int stacd)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.Start(stacd);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_tsk(ref T_RTSK pk_rtsk, ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.ReferStatus(ref pk_rtsk);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER wup_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.Wakeup();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER can_wup(ref int p_wupcnt, ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.CanWakeup(ref p_wupcnt);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER chg_pri(ID tskid, PRI tskpri)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.ChangePriority(tskpri);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER sus_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.Suspend();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER rsm_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.Resume();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER frsm_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.ForceResume();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER rel_wai(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.ReleaseWait() ? ER.E_OK : ER.E_CTX;
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ter_tsk(ID tskid)
		{
			ER Result = ER.E_NOEXS;
			Task Task;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Task = g_Kernel.Nucleus.GetTask(tskid);
				if (Task == null)
					Result = ER.E_NOEXS;
				else
					Result = Task.Terminate();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER cre_sem(ID semid, ref T_CSEM pk_csem, out ID p_semid)
		{
			ER Result = ER.E_NOEXS;

			p_semid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateSemaphore(semid, ref pk_csem, out p_semid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_sem(ID semid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteSemaphore(semid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER wai_sem(ID semid)
		{
			ER Result = ER.E_NOEXS;
			Semaphore Semaphore;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Semaphore = g_Kernel.Nucleus.GetSemaphore(semid);
				if (Semaphore == null)
					Result = ER.E_NOEXS;
				else
					Result = Semaphore.Wait(TMO.TMO_FEVR);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER preq_sem(ID semid)
		{
			ER Result = ER.E_NOEXS;
			Semaphore Semaphore;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Semaphore = g_Kernel.Nucleus.GetSemaphore(semid);
				if (Semaphore == null)
					Result = ER.E_NOEXS;
				else
					Result = Semaphore.Wait(TMO.TMO_POL);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER twai_sem(ID semid, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			Semaphore Semaphore;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Semaphore = g_Kernel.Nucleus.GetSemaphore(semid);
				if (Semaphore == null)
					Result = ER.E_NOEXS;
				else
					Result = Semaphore.Wait(tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER sig_sem(ID semid)
		{
			ER Result = ER.E_NOEXS;
			Semaphore Semaphore;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Semaphore = g_Kernel.Nucleus.GetSemaphore(semid);
				if (Semaphore == null)
					Result = ER.E_NOEXS;
				else
					Result = Semaphore.Signal();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_sem(ref T_RSEM pk_rsem, ID semid)
		{
			ER Result = ER.E_NOEXS;
			Semaphore Semaphore;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Semaphore = g_Kernel.Nucleus.GetSemaphore(semid);
				if (Semaphore == null)
					Result = ER.E_NOEXS;
				else
					Result = Semaphore.ReferStatus(ref pk_rsem);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER cre_flg(ID flgid, ref T_CFLG pk_cflg, out ID p_flgid)
		{
			ER Result = ER.E_NOEXS;

			p_flgid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateEventFlag(flgid, ref pk_cflg, out p_flgid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_flg(ID flgid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteEventFlag(flgid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER set_flg(ID flgid, FLGPTN setptn)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.SetEventFlag(setptn);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER clr_flg(ID flgid, FLGPTN clrptn)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.ClearEventFlag(clrptn);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER wai_flg(ref FLGPTN p_flgptn, ID flgid, FLGPTN waiptn, MODE wfmode)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.WaitEventFlag(ref p_flgptn, TMO.TMO_FEVR, waiptn, wfmode);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER pol_flg(ref FLGPTN p_flgptn, ID flgid, FLGPTN waiptn, MODE wfmode)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.WaitEventFlag(ref p_flgptn, TMO.TMO_POL, waiptn, wfmode);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER twai_flg(ref FLGPTN p_flgptn, ID flgid, TMO tmout, FLGPTN waiptn, MODE wfmode)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.WaitEventFlag(ref p_flgptn, tmout, waiptn, wfmode);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_flg(ref T_RFLG pk_rflg, ID flgid)
		{
			ER Result = ER.E_NOEXS;
			EventFlag EventFlag;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				EventFlag = g_Kernel.Nucleus.GetEventFlag(flgid);
				if (EventFlag == null)
					Result = ER.E_NOEXS;
				else
					Result = EventFlag.ReferStatus(ref pk_rflg);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER cre_mbx(ID mbxid, ref T_CMBX pk_cmbx, out ID p_mbxid)
		{
			ER Result = ER.E_NOEXS;

			p_mbxid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateMailbox(mbxid, ref pk_cmbx, out p_mbxid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER del_mbx(ID mbxid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteMailbox(mbxid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER snd_msg(ID mbxid, T_MSG pk_msg)
		{
			ER Result = ER.E_NOEXS;
			Mailbox Mailbox;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Mailbox = g_Kernel.Nucleus.GetMailbox(mbxid);
				if (Mailbox == null)
					Result = ER.E_NOEXS;
				else
					Result = Mailbox.SendMessage(ref pk_msg);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER rcv_msg(out T_MSG ppk_msg, ID mbxid)
		{
			ER Result = ER.E_NOEXS;
			Mailbox Mailbox;

			ppk_msg = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Mailbox = g_Kernel.Nucleus.GetMailbox(mbxid);
				if (Mailbox == null)
					Result = ER.E_NOEXS;
				else
					Result = Mailbox.ReceiveMessage(out ppk_msg, TMO.TMO_FEVR);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER prcv_msg(out T_MSG ppk_msg, ID mbxid)
		{
			ER Result = ER.E_NOEXS;
			Mailbox Mailbox;

			ppk_msg = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Mailbox = g_Kernel.Nucleus.GetMailbox(mbxid);
				if (Mailbox == null)
					Result = ER.E_NOEXS;
				else
					Result = Mailbox.ReceiveMessage(out ppk_msg, TMO.TMO_POL);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER trcv_msg(out T_MSG ppk_msg, ID mbxid, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			Mailbox Mailbox;

			ppk_msg = null;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Mailbox = g_Kernel.Nucleus.GetMailbox(mbxid);
				if (Mailbox == null)
					Result = ER.E_NOEXS;
				else
					Result = Mailbox.ReceiveMessage(out ppk_msg, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_mbx(ref T_RMBX pk_rmbx, ID mbxid)
		{
			ER Result = ER.E_NOEXS;
			Mailbox Mailbox;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Mailbox = g_Kernel.Nucleus.GetMailbox(mbxid);
				if (Mailbox == null)
					Result = ER.E_NOEXS;
				else
					Result = Mailbox.ReferStatus(ref pk_rmbx);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER def_cyc(HNO cycno, ref T_DCYC pk_dcyc)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DefineCyclicHandler(cycno, ref pk_dcyc);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER get_tim(out SYSTIME pk_tim)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null) {
				pk_tim = new SYSTIME();
				return ER.E_DLT;
			}

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.GetSystemTime(out pk_tim);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER set_tim(ref SYSTIME pk_tim)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.SetSystemTime(ref pk_tim);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER def_int(uint dintno, ref T_DINT pk_dint)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.DefineInterruptHandler(dintno, ref pk_dint);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER chg_icr(uint dintno, byte icrcmd)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.ChangeInterruptControlRegister(dintno, icrcmd);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER ref_sys(ref T_RSYS pk_rsys)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.ReferSystemStatus(ref pk_rsys);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER udp_cre_cep(ID cepid, ref T_UDP_CCEP pk_ccep, out ID p_cepid)
		{
			ER Result = ER.E_NOEXS;

			p_cepid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateUdpCep(cepid, ref pk_ccep, out p_cepid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER udp_del_cep(ID cepid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteUdpCep(cepid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER udp_snd_dat(ID cepid, T_IPV4EP p_dstaddr, pointer data, int len, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			UdpCep UdpCep;

			if ((data == null) || (len <= 0))
				return ER.E_PAR;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				UdpCep = g_Kernel.Nucleus.GetUdpCep(cepid);
				if (UdpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = UdpCep.SendData(p_dstaddr, data, len, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER udp_rcv_dat(ID cepid, T_IPV4EP p_dstaddr, pointer data, int len, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			UdpCep UdpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				UdpCep = g_Kernel.Nucleus.GetUdpCep(cepid);
				if (UdpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = UdpCep.ReceiveData(p_dstaddr, data, len, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_cre_rep(ID repid, ref T_TCP_CREP pk_crep, out ID p_repid)
		{
			ER Result = ER.E_NOEXS;

			p_repid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateTcpRep(repid, ref pk_crep, out p_repid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_del_rep(ID repid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteTcpRep(repid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_cre_cep(ID cepid, ref T_TCP_CCEP pk_ccep, out ID p_cepid)
		{
			ER Result = ER.E_NOEXS;

			p_cepid = ID.NULL;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.CreateTcpCep(cepid, ref pk_ccep, out p_cepid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_del_cep(ID cepid)
		{
			ER Result = ER.E_NOEXS;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				Result = g_Kernel.Nucleus.DeleteTcpCep(cepid);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_acp_cep(ID cepid, ID repid, T_IPV4EP p_dstaddr, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;
			TcpRep TcpRep;

			if (p_dstaddr == null)
				return ER.E_PAR;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else {
					TcpRep = g_Kernel.Nucleus.GetTcpRep(repid);
					if (TcpRep == null)
						Result = ER.E_NOEXS;
					else
						Result = TcpCep.Accept(TcpRep, p_dstaddr, tmout);
				}
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_con_cep(ID cepid, T_IPV4EP p_myaddr, T_IPV4EP p_dstaddr, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (p_dstaddr == null)
				return ER.E_PAR;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else 
					Result = TcpCep.Connect(p_myaddr, p_dstaddr, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_sht_cep(ID cepid)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.Shutdown();
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_cls_cep(ID cepid, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.Close(tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_snd_dat(ID cepid, pointer data, int len, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if ((data == null) || (len <= 0))
				return ER.E_PAR;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.SendData(data, len, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_rcv_dat(ID cepid, pointer data, int len, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.ReceiveData(data, len, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_get_buf(ID cepid, ref pointer p_buf, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.GetBuffer(ref p_buf, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_snd_buf(ID cepid, int len)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.SendBuffer(len);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_rcv_buf(ID cepid, ref pointer p_buf, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.ReceiveBuffer(ref p_buf, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_rel_buf(ID cepid, int len)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.ReleaseBuffer(len);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_snd_oob(ID cepid, pointer data, int len, TMO tmout)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.SendUrgentData(data, len, tmout);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_rcv_oob(ID cepid, pointer data, int len)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.ReceiveUrgentData(data, len);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_can_cep(ID cepid, FN fncd)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.Cancel(fncd);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_set_opt(ID cepid, int optname, pointer optval, int optlen)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.SetOption(optname, optval, optlen);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}

		public ER tcp_get_opt(ID cepid, int optname, pointer optval, int optlen)
		{
			ER Result = ER.E_NOEXS;
			TcpCep TcpCep;

			if (g_Kernel == null)
				return ER.E_DLT;

			g_Kernel.LockCPU();
			try {
				TcpCep = g_Kernel.Nucleus.GetTcpCep(cepid);
				if (TcpCep == null)
					Result = ER.E_NOEXS;
				else
					Result = TcpCep.GetOption(optname, optval, optlen);
			}
			finally {
				g_Kernel.UnlockCPU();
			}

			return Result;
		}
	}
}
