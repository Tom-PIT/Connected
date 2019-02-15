using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TomPIT.Caching;

namespace TomPIT.Connectivity
{
	internal static class HttpClientPool
	{
		public static HttpClient Get(ICredentials credentials, IInstanceMetadataProvider instanceProvider)
		{
			if (credentials is IBearerCredentials)
				return Get(((IBearerCredentials)credentials).Token, instanceProvider);

			var basic = credentials as IBasicCredentials;
			var token = string.IsNullOrWhiteSpace(basic.UserName) && string.IsNullOrWhiteSpace(basic.Password)
				? string.Empty
				: Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", basic.UserName, basic.Password)));

			var key = token;

			if (string.IsNullOrWhiteSpace(token))
				key = "anonimous";

			return MemoryCache.Default.Get("httpclient", key,
				(f) =>
				{
					f.SlidingExpiration = true;
					f.Duration = TimeSpan.FromMinutes(15);

					var r = new HttpClient();

					if (!string.IsNullOrWhiteSpace(token))
						r.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

					r.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					if (instanceProvider != null)
						r.DefaultRequestHeaders.Add("TomPITInstanceId", instanceProvider.InstanceId.ToString());

					r.Timeout = TimeSpan.FromSeconds(120);

					return r;
				});
		}

		public static HttpClient Get(string jwToken, IInstanceMetadataProvider instanceProvider)
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

					if (instanceProvider != null)
						r.DefaultRequestHeaders.Add("TomPITInstanceId", instanceProvider.InstanceId.ToString());

					r.Timeout = TimeSpan.FromSeconds(120);

					return r;
				});
		}
	}
}
