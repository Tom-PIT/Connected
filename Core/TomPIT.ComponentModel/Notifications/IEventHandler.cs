using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Notifications
{
	public interface IEventHandler : IProcessHandler
	{
		string EventName { get; }
		bool Cancel { get;  }
		void Invoke();
	}
}
