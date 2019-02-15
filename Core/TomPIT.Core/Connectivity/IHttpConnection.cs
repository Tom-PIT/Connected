using System.Net.Http;

namespace TomPIT.Connectivity
{
	public interface IHttpConnection
	{
		T Post<T>(string url, HttpRequestArgs e = null);
		T Post<T>(string url, object content, HttpRequestArgs e = null);
		T Post<T>(string url, HttpContent httpContent, HttpRequestArgs e = null);

		void Post(string url, HttpRequestArgs e = null);
		void Post(string url, object content, HttpRequestArgs e = null);
		void Post(string url, HttpContent httpContent, HttpRequestArgs e = null);

		T Get<T>(string url, HttpRequestArgs e = null);

		string AuthenticationToken { get; }
	}
}
