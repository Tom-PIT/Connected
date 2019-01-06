using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Connectivity
{
	public class SysConnection : ISysConnection, IDependencyInjector
	{
		private TokenValidationParameters _parameters = null;
		private CachingClient _cachingClient = null;
		private ServiceContainer _serviceContainer = null;
		private IMemoryCache _cache = null;

		public SysConnection(string url, string clientKey)
		{
			Url = url;
			ClientKey = clientKey;

			CachingClient.Connect();
		}

		public string Url { get; }
		private string ClientKey { get; }

		public IMemoryCache Cache
		{
			get
			{
				if (_cache == null)
					_cache = new MemoryCache();

				return _cache;
			}
		}

		private CachingClient CachingClient
		{
			get
			{
				if (_cachingClient == null)
					_cachingClient = new CachingClient(this);

				return _cachingClient;
			}
		}

		private ServiceContainer Services
		{
			get
			{
				if (_serviceContainer == null)
					_serviceContainer = new ServiceContainer(this);

				return _serviceContainer;
			}
		}

		public TokenValidationParameters ValidationParameters
		{
			get
			{
				if (_parameters == null)
				{
					var url = this.CreateUrl("Security", "SelectValidationParameters");
					var p = Get<ValidationParameters>(url);

					_parameters = new TokenValidationParameters
					{
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(p.IssuerSigningKey)),
						ValidAudience = p.ValidAudience,
						ValidIssuer = p.ValidIssuer
					};
				}

				return _parameters;
			}
		}

		public T GetService<T>()
		{
			return Services.Get<T>();
		}

		public void RegisterService(Type contract, object instance)
		{
			Services.Register(contract, instance);
		}

		public bool ResolveParameter(Type type, out object instance)
		{
			if (type == typeof(ISysConnection))
			{
				instance = this;
				return true;
			}

			instance = null;
			return false;
		}

		private ISysConnection Context { get; }

		public T Get<T>(string url)
		{
			try
			{
				var client = HttpClientPool.Get(ClientKey);
				var response = client.GetAsync(url).GetAwaiter().GetResult();

				return HandleResponse<T>(response);
			}
			catch (Exception ex)
			{
				throw Unwrap(ex);
			}
		}

		public T Post<T>(string url)
		{
			return Post<T>(url, null);
		}

		public T Post<T>(string url, object content)
		{
			return Post<T>(url, CreateContent(content));
		}

		public void Post(string url)
		{
			Post(url, null);
		}

		public void Post(string url, object content)
		{
			var client = HttpClientPool.Get(ClientKey);

			HandleResponse(client.PostAsync(url, CreateContent(content)).GetAwaiter().GetResult());
		}

		public void Post(string url, HttpContent httpContent)
		{
			var client = HttpClientPool.Get(ClientKey);

			HandleResponse(client.PostAsync(url, httpContent).GetAwaiter().GetResult());
		}

		public T Post<T>(string url, HttpContent httpContent)
		{
			var client = HttpClientPool.Get(ClientKey);

			return HandleResponse<T>(client.PostAsync(url, httpContent).GetAwaiter().GetResult());
		}

		private void HandleResponse(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
				HandleResponseException(response);
		}

		private T HandleResponse<T>(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
				HandleResponseException(response);

			var content = response.Content.ReadAsStringAsync().Result;

			if (IsNull(content))
				return default(T);

			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
			};

			return JsonConvert.DeserializeObject<T>(content, settings);
		}

		private void HandleResponseException(HttpResponseMessage response)
		{
			var rt = string.Empty;

			if (response.Content != null)
				rt = response.Content.ReadAsStringAsync().Result;

			JObject ex = null;

			try
			{
				ex = JsonConvert.DeserializeObject(rt) as JObject;
			}
			catch
			{
				throw new Exception(response.ReasonPhrase);
			}

			if (ex == null)
				throw new Exception(response.ReasonPhrase);

			var source = string.Empty;
			var message = string.Empty;

			if (ex.ContainsKey("source"))
				source = ex.Value<string>("source");

			if (ex.ContainsKey("message"))
				message = ex.Value<string>("message");

			throw new TomPITException(source, message);
		}

		private Exception Unwrap(Exception ex)
		{
			if (ex is AggregateException)
			{
				var baseEx = ex.InnerException;

				if (baseEx == null)
					return ex;

				while (baseEx != null)
				{
					var be = baseEx.InnerException;

					if (be == null)
						return baseEx;

					baseEx = be;
				}

				return baseEx;
			}

			return ex;
		}

		private HttpContent CreateContent(object content)
		{
			if (content == null || Convert.IsDBNull(content))
				return new StringContent(string.Empty);

			var c = JsonConvert.SerializeObject(content);

			content = CompressString(c);

			var sc = new StringContent(c, Encoding.UTF8, "application/json");

			sc.Headers.Add("Content-Encoding", "gzip");

			return sc;
		}

		private string CompressString(string text)
		{
			var buffer = Encoding.UTF8.GetBytes(text);

			using (var ms = new MemoryStream())
			{
				using (var gZipStream = new GZipStream(ms, CompressionMode.Compress, true))
				{
					gZipStream.Write(buffer, 0, buffer.Length);
				}

				ms.Position = 0;

				var compressedData = new byte[ms.Length];

				ms.Read(compressedData, 0, compressedData.Length);

				var gZipBuffer = new byte[compressedData.Length + 4];

				Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
				Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);

				return Convert.ToBase64String(gZipBuffer);
			}
		}

		private static bool IsNull(string content)
		{
			return string.Compare(content, "null", true) == 0
				|| string.IsNullOrWhiteSpace(content);
		}
	}
}
