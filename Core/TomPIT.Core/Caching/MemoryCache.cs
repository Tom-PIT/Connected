using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Caching
{
	public class MemoryCache : IMemoryCache, IDisposable
	{
		private static Lazy<MemoryCache> _default = new Lazy<MemoryCache>();

		public event CacheInvalidateHandler Invalidating;
		public event CacheInvalidateHandler Invalidate;
		public event CacheInvalidateHandler Invalidated;

		private Lazy<Container> _container = new Lazy<Container>();
		private CancellationTokenSource _cancel = new CancellationTokenSource();

		private Container Container { get { return _container.Value; } }

		public MemoryCache()
		{
			new Task(() => OnScaveging(), _cancel.Token, TaskCreationOptions.LongRunning).Start();
		}

		private void OnScaveging()
		{
			while (!_cancel.Token.IsCancellationRequested)
			{
				try
				{
					Container.Scave();

					_cancel.Token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
				}
				catch { }
			}
		}

		public bool IsEmpty(string key)
		{
			return Container.IsEmpty(key);
		}

		public bool Exists(string key)
		{
			return Container.Exists(key);
		}

		public void CreateKey(string key)
		{
			Container.CreateKey(key);
		}

		public List<T> All<T>(string key) where T : class
		{
			List<T> r = Container.All<T>(key);

			if (r == null)
				return new List<T>();

			return r;
		}

		public int Count(string key)
		{
			return Container.Count(key);
		}

		public T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			var r = Container.Get(key, id);

			if (r == null)
			{
				if (retrieve == null)
					return null;

				var options = new EntryOptions
				{
					Key = id
				};

				T instance = retrieve(options);

				if (EqualityComparer<T>.Default.Equals(instance, default(T)))
				{
					if (!options.AllowNull)
						return null;
				}

				Set(key, options.Key, instance, options.Duration, options.SlidingExpiration);

				return instance;
			}
			else
				return (T)r.Instance;
		}

		public void Clear(string key)
		{
			Container.Remove(key);
		}

		public T Get<T>(string key, string id) where T : class
		{
			Entry ce = Container.Get(key, id);

			if (ce == null || ce.Instance == null)
				return default(T);

			return (T)ce.Instance;
		}

		public T Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			Entry ce = Container.Get<T>(key, predicate);

			if (ce == null || ce.Instance == null)
				return default(T);

			return (T)ce.Instance;
		}

		public T First<T>(string key) where T : class
		{
			Entry ce = Container.First(key);

			if (ce == null || ce.Instance == null)
				return default(T);

			return (T)ce.Instance;
		}

		public List<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			var r = Container.Where<T>(key, predicate);

			if (r == null)
				return new List<T>();

			return r;
		}

		public T Set<T>(string key, string id, T instance)
		{
			return Set(key, id, instance, TimeSpan.Zero);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration)
		{
			return Set(key, id, instance, duration, false);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration)
		{
			Container.Set(key, id, instance, duration, slidingExpiration);

			return instance;
		}

		public void Remove(string key, string id)
		{
			Container.Remove(key, id);
		}

		public void Refresh(string key, string id)
		{
			var existing = Get<object>(key, id);
			var args = new CacheEventArgs(id, key);

			Invalidating?.Invoke(args);
			Invalidate?.Invoke(args);

			var newInstance = Get<object>(key, id);

			if (existing != null)
			{
				if (newInstance == null)
					Container.Remove(key, id);
				else if (existing.Equals(newInstance))
					Container.Remove(key, id);
			}

			Invalidated?.Invoke(args);
		}

		public void Dispose()
		{
			Container.Clear();
		}

		public string GenerateKey(params object[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
				return null;

			var sb = new StringBuilder();

			foreach (object i in parameters)
			{
				if (Types.TryConvertInvariant(i, out string s))
					sb.AppendFormat("{0}.", s);
			}

			return sb.ToString().TrimEnd('.');
		}

		public void Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			Container.Remove(key, predicate);
		}

		public string GenerateRandomKey(string key)
		{
			var keys = Container.Keys(key);
			var result = Generate();

			while (true)
			{
				if (keys == null || !keys.Contains(result))
					break;
			}

			return result;
		}

		private string Generate()
		{
			string chars = "abcdefghijklmnopqrstuvzxyw0123456789";
			var r = new Random();
			var sb = new StringBuilder("_");

			for (var i = 0; i < 7; i++)
				sb.Append(chars[r.Next(chars.Length - 1)]);

			return sb.ToString();
		}

		public static MemoryCache Default { get { return _default.Value; } }
	}
}