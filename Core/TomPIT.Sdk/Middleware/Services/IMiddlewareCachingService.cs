using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCachingService
	{
		bool Exists(string key);
		bool IsEmpty(string key);
		void CreateKey(string key);
		List<T> All<T>(string key) where T : class;
		T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class;
		void Clear(string key);
		T Get<T>(string key, string id) where T : class;
		T Get<T>(string key, Func<T, bool> predicate) where T : class;
		T First<T>(string key) where T : class;

		List<T> Where<T>(string key, Func<T, bool> predicate) where T : class;
		T Set<T>(string key, string id, T instance);
		T Set<T>(string key, string id, T instance, TimeSpan duration);
		T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration);
		void Remove(string key, string id);
		void Remove<T>(string key, Func<T, bool> predicate) where T : class;
		int Count(string key);

		string GenerateKey(params object[] parameters);
		string GenerateRandomKey(string key);
	}
}
