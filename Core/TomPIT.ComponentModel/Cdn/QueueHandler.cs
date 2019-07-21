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

		public virtual QueueValidationBehavior ValidationFailed => QueueValidationBehavior.Retry;

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
