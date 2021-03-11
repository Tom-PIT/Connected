using System;
using System.Collections.Generic;
using TomPIT.Middleware;

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
		List<T> All<T>(IMiddlewareContext context, string key) where T : class;
		T Get<T>(IMiddlewareContext context, string key, Func<T, bool> matchEvaluator, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(IMiddlewareContext context, string key, string id, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(IMiddlewareContext context, string key, string id) where T : class;
		T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class;
		T First<T>(IMiddlewareContext context, string key) where T : class;
		List<T> Where<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class;
		T Set<T>(string key, string id, T instance) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class;
		int Count(string key);
		string GenerateKey(params object[] parameters);
		string GenerateRandomKey(string key);

		void Reset(string cacheKey);
	}
}
