using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using TomPIT.Caching;

namespace TomPIT.Connectivity
{
	public interface ISysConnection : IHttpConnection
	{
		string Url { get; }

		IMemoryCache Cache { get; }
		T GetService<T>();
		void RegisterService(Type contract, object instance);
		TokenValidationParameters ValidationParameters { get; }

		ConcurrentDictionary<string, object> Items { get; }
	}
}
