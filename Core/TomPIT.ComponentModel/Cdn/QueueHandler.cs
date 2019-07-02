using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public abstract class QueueHandler : ProcessHandler, IQueueHandler
	{
		public QueueHandler(IDataModelContext context) : base(context)
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
