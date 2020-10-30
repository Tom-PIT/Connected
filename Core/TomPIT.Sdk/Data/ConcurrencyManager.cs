using System;
using TomPIT.Exceptions;

namespace TomPIT.Data
{
	public class ConcurrencyManager
	{
		public ConcurrencyManager(Action invokeAction, Action reloadAction) : this(invokeAction, reloadAction, 3)
		{
		}

		public ConcurrencyManager(Action invokeAction, Action reloadAction, int retryCount)
		{
			InvokeAction = invokeAction;
			ReloadAction = reloadAction;
			RetryCount = retryCount;

			Execute();
		}

		public static void Invoke(Action invoke, Action reload)
		{
			new ConcurrencyManager(invoke, reload);
		}

		private Action InvokeAction { get; }
		private Action ReloadAction { get; }

		public int RetryCount { get; } = 3;

		private void Execute()
		{
			ConcurrencyException lastException = null;

			for (var i = 0; i < RetryCount; i++)
			{
				try
				{
					InvokeAction();
					return;
				}
				catch (ConcurrencyException ex)
				{
					lastException = ex;
					ReloadAction();
				}
			}

			throw lastException;
		}
	}
}
