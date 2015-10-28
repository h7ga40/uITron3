using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public class StateMachine<ST> : IStateMachine
	{
		public delegate void TTimeOutEvent(object data);

		ST m_State;
		TMO m_Timer;						// タイマー値
		TMO m_TimeOut;						// タイムアウト値
		object m_TimeOutData;				// タイムアウト処理関数に渡すデータ
		TTimeOutEvent m_OnTimeOut;			// タイムアウト処理（引数msgがnullなので注意）
		object m_Data;						// ユーザーデータ

		public StateMachine()
		{
			m_Timer = TMO.TMO_FEVR;
			m_TimeOut = TMO.TMO_FEVR;
			m_TimeOutData = 0;
			m_OnTimeOut = null;
		}

		object IStateMachine.State { get { return m_State; } }

		public ST State { get { return m_State; } }

		public TMO Timer { get { return m_Timer; } }

		public object Data { get { return m_Data; } set { m_Data = value; } }

		protected void SetState(ST state)
		{
			m_State = state;
			m_Timer = TMO.TMO_FEVR;
			m_TimeOut = TMO.TMO_FEVR;
			m_TimeOutData = 0;
			m_OnTimeOut = null;
		}

		protected void SetState(ST state, TMO timeOut, object timeOutData,
			TTimeOutEvent onTimeOut)
		{
			m_State = state;
			m_Timer = timeOut;
			m_TimeOut = timeOut;
			m_TimeOutData = timeOutData;
			m_OnTimeOut = onTimeOut;
		}

		public void Progress(TMO interval)
		{
			if (m_TimeOut == TMO.TMO_FEVR) {
				m_Timer = TMO.TMO_FEVR;
				return;
			}

			m_Timer.Value -= interval;
			if (m_Timer <= 0) {
				m_Timer.Value = 0;
			}
		}

		public void CallTimeOut()
		{
			if (m_Timer == 0) {
				// タイムアウト処理関数呼び出し
				if (m_OnTimeOut != null) {
					m_OnTimeOut(m_TimeOutData);
				}
				// 新タイムアウト値設定
				m_Timer = m_TimeOut;
			}
		}
	}
}
