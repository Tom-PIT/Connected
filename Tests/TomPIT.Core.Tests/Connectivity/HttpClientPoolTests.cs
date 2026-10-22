using System;
using System.Net.Http;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Connectivity;
using Xunit;

namespace TomPIT.Core.Tests.Connectivity
{
	public class HttpClientPoolTests
	{
		[Fact]
		public void Get_SameToken_ReturnsPooledClient()
		{
			var token = Guid.NewGuid().ToString();

			Assert.Same(HttpClientPool.Get(token, null), HttpClientPool.Get(token, null));
		}

		[Fact]
		public async Task EvictedClient_IsNotDisposed()
		{
			var token = Guid.NewGuid().ToString();
			var client = HttpClientPool.Get(token, null);
			/*
			 * Removing the entry goes through the same disposal as expiry.
			 */
			MemoryCache.Default.Remove("httpclient", token);

			Assert.NotSame(client, HttpClientPool.Get(token, null));

			var ex = await Record.ExceptionAsync(() => client.GetAsync("http://127.0.0.1:9/"));

			Assert.IsNotType<ObjectDisposedException>(ex);
		}
	}
}
