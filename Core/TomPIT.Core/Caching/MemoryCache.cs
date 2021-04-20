using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Exceptions;

namespace TomPIT.Caching
{
	public sealed class MemoryCache : IMemoryCache, IDisposable
	{
		public event CacheInvalidateHandler Invalidating;
		public event CacheInvalidateHandler Invalidate;
		public event CacheInvalidateHandler Invalidated;

		private static readonly Lazy<MemoryCache> _default = new Lazy<MemoryCache>();
		private readonly Lazy<Container> _container = new Lazy<Container>();
		private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

		public MemoryCache():this( CacheScope.Shared)
		{

		}
		public MemoryCache(CacheScope scope)
		{
			Scope = scope;

			if (scope == CacheScope.Shared)
				new Task(() => OnScaveging(), _cancel.Token, TaskCreationOptions.LongRunning).Start();
		}

		private CacheScope Scope { get; }

		private Container Container => _container.Value;
		private CancellationTokenSource Cancel => _cancel;
		private void OnScaveging()
		{
			var token = Cancel.Token;

			while (!token.IsCancellationRequested)
			{
				try
				{
					Container.Scave();

					token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
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

		public IEnumerator<T> GetEnumerator<T>(string key) where T : class
		{
			return Container.GetEnumerator<T>(key);
		}
		public ImmutableList<T> All<T>(string key) where T : class
		{
			var r = Container.All<T>(key);

			if (r == null)
				return ImmutableList<T>.Empty;

			return r;
		}

		public int Count(string key)
		{
			return Container.Count(key);
		}

		public T Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Get(key, predicate, null);
		}
		public T Get<T>(string key, Func<T, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			var r = Container.Get(key, predicate);

			if (r == null)
			{
				if (retrieve is null)
					return null;

				var options = new EntryOptions();
				T instance = retrieve(options);

				if (EqualityComparer<T>.Default.Equals(instance, default))
				{
					if (!options.AllowNull)
						return null;
				}

				if (CanStore(options))
				{
					if (string.IsNullOrWhiteSpace(options.Key))
						throw new TomPITException(SR.ErrCacheKeyNull);

					Set(key, options.Key, instance, options.Duration, options.SlidingExpiration, options.Scope);
				}

				return instance;
			}
			else
				return (T)r.Instance;
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

				if (EqualityComparer<T>.Default.Equals(instance, default))
				{
					if (!options.AllowNull)
						return null;
				}

				if (CanStore(options))
					Set(key, options.Key, instance, options.Duration, options.SlidingExpiration, options.Scope);

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
			var ce = Container.Get(key, id);

			return ce == null || ce.Instance == null ? default : (T)ce.Instance;
		}

		public T Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			var ce = Container.Get<T>(key, predicate);

			return ce == null || ce.Instance == null ? default : (T)ce.Instance;
		}

		public T First<T>(string key) where T : class
		{
			var ce = Container.First(key);

			return ce == null || ce.Instance == null ? default : (T)ce.Instance;
		}

		public ImmutableList<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			var r = Container.Where(key, predicate);

			if (r == null)
				return ImmutableList<T>.Empty;

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
			return Set(key, id, instance, duration, slidingExpiration, CacheScope.Shared);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration, CacheScope scope)
		{
			Container.Set(key, id, instance, duration, slidingExpiration, scope);

			return instance;
		}

		public void Remove(string key, string id)
		{
			Container.Remove(key, id);
		}

		/// <summary>
		/// This operation must be synchronous otherwise it can cause
		/// inconsistent caching image.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="id"></param>
		public void Refresh(string key, string id)
		{
			/*
			 * we store existing instance but it is not
			 * removed from the cache yet. This is because other
			 * threads can access this instance while we are
			 * retrieving a new version from the server
			 */
			var existing = Get<object>(key, id);
			var args = new CacheEventArgs(id, key);
			/*
			 * this two events invalidate that cache reference.
			 * note that if no new version exists the existing one
			 * is still available to other threads.
			 */
			Invalidating?.Invoke(args);
			Invalidate?.Invoke(args);
			/*
			 * now find out if a new version has been set for the
			 * specified key
			 */
			var newInstance = Get<object>(key, id);
			/*
			 * if no existing reference exists there is no need for
			 * removing anything
			 */
			if (existing != null)
			{
				/*
				 * we have an existing instance. we are dealing with two possible scenarios:
				 * - newInstance is null because entity has been deleted
				 * - newInstance is actually the same instance as the existing which means a new
				 * version does not exist. In both cases we must remove existing reference because
				 * at this point it is not valid anymore.
				 * note that the third case exists: reference has been replaced. in that case there
				 * is nothing to do because Invalidating events has already replaced reference with a
				 * new version.
				 */
				if (newInstance == null)
					Container.Remove(key, id);
				else if (existing.Equals(newInstance) && args.InvalidateBehavior == InvalidateBehavior.RemoveSameInstance)
					Container.Remove(key, id);
			}

			Invalidated?.Invoke(args);
		}

		public void Dispose()
		{
			Cancel.Cancel();
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

		public List<string> Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Container.Remove(key, predicate);
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

		private static string Generate()
		{
			string chars = "abcdefghijklmnopqrstuvzxyw0123456789";
			var r = new Random();
			var sb = new StringBuilder("_");

			for (var i = 0; i < 7; i++)
				sb.Append(chars[r.Next(chars.Length - 1)]);

			return sb.ToString();
		}

		public ImmutableList<string> Keys(string key)
		{
			return Container.Keys(key).ToImmutableList();
		}

		public ImmutableList<string> Keys()
		{
			return Container.Keys().ToImmutableList();
		}

		public static MemoryCache Default => _default.Value;

		private bool CanStore(EntryOptions options)
		{
			if (Scope == CacheScope.Context)
				return true;

			return options.Scope != CacheScope.Context;
		}

		public CacheScope GetScope(string key, string id)
		{
			var ce = Container.Get(key, id);

			return ce is null ? CacheScope.Shared : ce.Scope;
		}

		public void Merge(IMemoryCache cache)
		{
			if (cache is not MemoryCache mc)
				throw new ArgumentException(null, nameof(cache));

			foreach (var key in mc.Keys())
			{
				foreach (var entryKey in mc.Keys(key))
				{
					if (mc.GetScope(key, entryKey) == CacheScope.Shared)
						Move(key, entryKey, mc.Container.Get(key, entryKey));
				}
			}
		}

		private void Move(string key, string id, Entry value)
		{
			Container.Set(key, id, value);
		}
	}
}