﻿using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareCachingService : IDisposable
	{
		bool Exists(string key);
		bool IsEmpty(string key);
		void CreateKey(string key);
		List<T> All<T>(string key) where T : class;
		T Get<T>(string key, Func<T, bool> matchEvaluator, CacheRetrieveHandler<T> retrieve) where T : class;
		T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class;
		void Clear(string key);
		T Get<T>(string key, string id) where T : class;
		T Get<T>(string key, Func<dynamic, bool> predicate) where T : class;
		T Get<T>(string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class;
		T First<T>(string key) where T : class;

		List<T> Where<T>(string key, Func<dynamic, bool> predicate) where T : class;
		T Set<T>(string key, string id, T instance) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class;
		T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class;
		void Remove(string key, string id);
		void Remove(string key, List<string> ids);
		void Remove<T>(string key, Func<dynamic, bool> predicate) where T : class;
		int Count(string key);
		void Reset(string key);
		string GenerateKey(params object[] parameters);
		string GenerateRandomKey(string key);

		void Flush();
	}
}
