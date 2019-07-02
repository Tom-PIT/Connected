using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public interface IQueueHandler : IProcessHandler
	{
		void Invoke();
	}
}
