using System;
using System.Threading;
using TomPIT.Exceptions;

namespace TomPIT.Data
{
	public class ConcurrencyManager
	{
		public ConcurrencyManager(Action invokeAction, Action reloadAction) : this(invokeAction, reloadAction, 5)
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

		public static void Invoke(Action invoke, Action reload, int retryCount)
		{
			new ConcurrencyManager(invoke, reload, retryCount);
		}

		private Action InvokeAction { get; }
		private Action ReloadAction { get; }

		public int RetryCount { get; } = 5;

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

					Thread.Sleep(i * i * 50);
					
					ReloadAction();
				}
			}

			throw lastException;
		}
	}
}
