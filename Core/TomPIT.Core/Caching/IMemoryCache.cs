using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TomPIT.Caching
{
	public delegate void CacheInvalidateHandler(CacheEventArgs e);
	public delegate T CacheRetrieveHandler<T>(EntryOptions options);

	public enum CacheScope : byte
	{
		Shared = 1,
		Context = 2
	}

	public interface IMemoryCache
	{
		event CacheInvalidateHandler Invalidating;
		event CacheInvalidateHandler Invalidate;
		event CacheInvalidateHandler Invalidated;

		bool IsEmpty(string key);
		bool Exists(string key);
		void CreateKey(string key);
		ImmutableList<T> All<T>(string key) where T : class;
		T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class;
		void Clear(string key);
		IEnumerator<T> GetEnumerator<T>(string key) where T : class;
		T Get<T>(string key, string id) where T : class;
		T Get<T>(string key, Func<T, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(string key, Func<dynamic, bool> predicate) where T : class;
		T First<T>(string key) where T : class;

		ImmutableList<T> Where<T>(string key, Func<T, bool> predicate) where T : class;
		T Set<T>(string key, string id, T instance);
		T Set<T>(string key, string id, T instance, TimeSpan duration);
		T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration);
		void Remove(string key, string id);
		List<string> Remove<T>(string key, Func<T, bool> predicate) where T : class;
		void Refresh(string key, string id);

		string GenerateKey(params object[] parameters);
		string GenerateRandomKey(string key);

		int Count(string key);
		ImmutableList<string> Keys(string key);

		void Merge(IMemoryCache cache);
	}
}
