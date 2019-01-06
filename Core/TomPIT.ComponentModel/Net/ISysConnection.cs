using System.Net.Http;

namespace TomPIT.Net
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
	}
}
