using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Workers
{
	public interface IHostedWorkerHandler : IProcessHandler
	{
		void Invoke();
	}
}
