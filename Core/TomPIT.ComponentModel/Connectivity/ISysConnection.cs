using Microsoft.IdentityModel.Tokens;
using System;
using System.Net.Http;
using TomPIT.Caching;

namespace TomPIT.Connectivity
{
	public interface ISysConnection
	{
		T Post<T>(string url, HttpRequestArgs e = null);
		T Post<T>(string url, object content, HttpRequestArgs e = null);
		T Post<T>(string url, HttpContent httpContent, HttpRequestArgs e = null);

		void Post(string url, HttpRequestArgs e = null);
		void Post(string url, object content, HttpRequestArgs e = null);
		void Post(string url, HttpContent httpContent, HttpRequestArgs e = null);

		T Get<T>(string url, HttpRequestArgs e = null);

		string Url { get; }

		IMemoryCache Cache { get; }
		T GetService<T>();
		void RegisterService(Type contract, object instance);

		TokenValidationParameters ValidationParameters { get; }
	}
}
