using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uITron3
{
	public interface IStateMachine
	{
		object State { get; }
		TMO Timer { get; }
		object Data { get; set; }
		void Progress(TMO interval);
		void CallTimeOut();
	}
}
