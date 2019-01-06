using Microsoft.IdentityModel.Tokens;
using System;
using System.Net.Http;
using TomPIT.Caching;

namespace TomPIT.Connectivity
{
	public interface ISysConnection
	{
		T Post<T>(string url);
		T Post<T>(string url, object content);
		T Post<T>(string url, HttpContent httpContent);

		void Post(string url);
		void Post(string url, object content);
		void Post(string url, HttpContent httpContent);

		T Get<T>(string url);

		string Url { get; }

		IMemoryCache Cache { get; }
		T GetService<T>();
		void RegisterService(Type contract, object instance);

		TokenValidationParameters ValidationParameters { get; }
	}
}
