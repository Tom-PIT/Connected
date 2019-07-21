using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	public enum QueueValidationBehavior
	{
		Retry = 1,
		Complete = 2
	}
	public interface IQueueHandler : IProcessHandler
	{
		void Invoke();

		QueueValidationBehavior ValidationFailed { get; }
	}
}
