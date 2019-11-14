using System;
using System.Collections.Generic;

namespace TomPIT.Caching
{
	public interface IDataCachingService
	{
		void Clear(string cacheKey);
		void Remove(string cacheKey, List<string> ids);
		void Invalidate(string cacheKey, List<string> ids);

		void RegisterHandler(string cacheKey, IDataCachingHandler handler);

		bool Exists(string key);
		bool IsEmpty(string key);
		void CreateKey(string key);
		List<T> All<T>(string key) where T : class;
		T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(string key, string id) where T : class;
		T Get<T>(string key, Func<dynamic, bool> predicate) where T : class;
		T First<T>(string key) where T : class;
		List<T> Where<T>(string key, Func<dynamic, bool> predicate) where T : class;
		T Set<T>(string key, string id, T instance) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class;
		int Count(string key);
		string GenerateKey(params object[] parameters);
		string GenerateRandomKey(string key);

		void Reset(string cacheKey);
	}
}
