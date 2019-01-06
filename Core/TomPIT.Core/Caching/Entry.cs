using System;

namespace TomPIT.Caching
{
	internal class Entry : IDisposable
	{
		private DateTime _expirationDate = DateTime.MinValue;
		private bool _slidingExpiration = false;
		private TimeSpan _duration;

		public Entry(string id, object instance, TimeSpan duration, bool slidingExpiration)
		{
			Id = id;
			_duration = duration;
			Instance = instance;
			_slidingExpiration = slidingExpiration;

			if (duration > TimeSpan.Zero)
				_expirationDate = DateTime.UtcNow.AddTicks(duration.Ticks);
		}

		public object Instance { get; }
		public string Id { get; }

		public void Hit()
		{
			if (_slidingExpiration && _duration > TimeSpan.Zero)
				_expirationDate = DateTime.UtcNow.AddTicks(_duration.Ticks);
		}

		public bool Expired
		{
			get
			{
				if (_expirationDate == DateTime.MinValue)
					return false;

				return _expirationDate < DateTime.UtcNow;
			}
		}

		public void Dispose()
		{
			if (Instance != null && Instance is IDisposable)
				((IDisposable)Instance).Dispose();
		}
	}
}