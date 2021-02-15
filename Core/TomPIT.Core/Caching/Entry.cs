using System;

namespace TomPIT.Caching
{
	internal class Entry : IDisposable
	{
		public Entry(string id, object instance, TimeSpan duration, bool slidingExpiration)
		{
			Id = id;
			Instance = instance;
			SlidingExpiration = slidingExpiration;
			Duration = duration;

			if (Duration > TimeSpan.Zero)
				ExpirationDate = DateTime.UtcNow.AddTicks(duration.Ticks);
		}

		private bool SlidingExpiration { get; }
		private DateTime ExpirationDate { get; set; }
		private TimeSpan Duration { get; set; }

		public object Instance { get; }
		public string Id { get; }
		public bool Expired => ExpirationDate != DateTime.MinValue && ExpirationDate < DateTime.UtcNow;

		public void Hit()
		{
			if (SlidingExpiration && Duration > TimeSpan.Zero)
				ExpirationDate = DateTime.UtcNow.AddTicks(Duration.Ticks);
		}

		public void Dispose()
		{
			if (Instance is IDisposable disposable)
				disposable.Dispose();
		}
	}
}