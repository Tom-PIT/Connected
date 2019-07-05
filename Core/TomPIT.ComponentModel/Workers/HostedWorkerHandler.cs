using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Workers
{
	public abstract class HostedWorkerHandler : ProcessHandler, IHostedWorkerHandler
	{
		public HostedWorkerHandler(IDataModelContext context) : base(context)
		{

		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
