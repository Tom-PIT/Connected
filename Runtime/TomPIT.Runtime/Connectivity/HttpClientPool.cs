using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TomPIT.Caching;

namespace TomPIT.Net
{
	internal static class HttpClientPool
	{
		public static HttpClient Get(string jwToken)
		{
			var token = jwToken;

			if (string.IsNullOrWhiteSpace(jwToken))
				token = "anonimous";

			return MemoryCache.Default.Get("httpclient", token,
				(f) =>
				{
					f.SlidingExpiration = true;
					f.Duration = TimeSpan.FromMinutes(15);

					var r = new HttpClient();

					if (!string.IsNullOrWhiteSpace(jwToken))
						r.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwToken);

					r.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					r.Timeout = TimeSpan.FromSeconds(120);

					return r;
				});
		}
	}
}
