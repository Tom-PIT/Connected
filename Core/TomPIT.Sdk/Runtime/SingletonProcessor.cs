using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TomPIT.Runtime
{
	public class SingletonProcessor<T>
	{
		private Lazy<ConcurrentDictionary<T, ManualResetEvent>> _state= new Lazy<ConcurrentDictionary<T, ManualResetEvent>>();
		private bool _isInitialized = false;

		public void Start(T value, Action create, Action retrieve)
		{
			var resetEvent = new ManualResetEvent(false);

			if (!State.TryAdd(value, resetEvent))
			{
				resetEvent.Dispose();

				if (State.TryGetValue(value, out resetEvent))
					resetEvent.WaitOne();

				retrieve();
			}
			else
			{
				try
				{
					_isInitialized = true;
					create();
				}
				finally
				{
					resetEvent.Set();

					if (State.TryRemove(value, out ManualResetEvent e))
						e.Dispose();
				}
			}
		}

		public void Start(T value, Action create)
		{
			var resetEvent = new ManualResetEvent(false);

			if (!State.TryAdd(value, resetEvent))
			{
				resetEvent.Dispose();

				if (State.TryGetValue(value, out resetEvent))
					resetEvent.WaitOne();
			}
			else
			{
				try
				{
					_isInitialized = true;
					create();
				}
				finally
				{
					resetEvent.Set();

					if (State.TryRemove(value, out ManualResetEvent e))
						e.Dispose();
				}
			}
		}

		private ConcurrentDictionary<T, ManualResetEvent> State => _state.Value;

		public bool IsInitialized => _isInitialized;
	}
}
