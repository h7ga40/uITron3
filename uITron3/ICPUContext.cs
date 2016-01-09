using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uITron3
{
	internal interface ICPUContext
	{
		bool IsCurrent();
		Task GetTask();
		void Activate(Task task, TTaskExecute pExecute);
		bool Dispatch();
		void PushContext();
		void PopContext();
		void Exit();
	}
}
