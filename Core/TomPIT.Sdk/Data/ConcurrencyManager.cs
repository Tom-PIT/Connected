using System;
using TomPIT.Exceptions;

namespace TomPIT.Data
{
	public class ConcurrencyManager
	{
		public ConcurrencyManager(Action invoke, Action reload) : this(invoke, reload, 3)
		{
		}

		public ConcurrencyManager(Action invoke, Action reload, int retryCount)
		{
			Invoke = invoke;
			Reload = reload;
			RetryCount = retryCount;

			Execute();
		}

		private Action Invoke { get; }
		private Action Reload { get; }

		public int RetryCount { get; } = 3;

		private void Execute()
		{
			ConcurrencyException lastException = null;

			for (var i = 0; i < RetryCount; i++)
			{
				try
				{
					Invoke();
					return;
				}
				catch (ConcurrencyException ex)
				{
					lastException = ex;
					Reload();
				}
			}

			throw lastException;
		}
	}
}
