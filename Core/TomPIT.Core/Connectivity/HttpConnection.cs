using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using TomPIT.Exceptions;
using TomPIT.Serialization;

namespace TomPIT.Connectivity
{
	public class HttpConnection : IHttpConnection
	{
		public HttpConnection()
		{
		}

		public HttpConnection(string authenticationToken)
		{
			AuthenticationToken = authenticationToken;
		}

		public string AuthenticationToken { get; }

		public T Get<T>(string url, HttpRequestArgs e = null)
		{
			try
			{
				var client = e == null || e.Credentials == null
					? HttpClientPool.Get(AuthenticationToken, this as IInstanceMetadataProvider)
					: HttpClientPool.Get(e.Credentials, this as IInstanceMetadataProvider);

				var response = client.GetAsync(url).GetAwaiter().GetResult();

				return HandleResponse<T>(response, e);
			}
			catch (Exception ex)
			{
				throw Unwrap(ex);
			}
		}

		public T Post<T>(string url, HttpRequestArgs e = null)
		{
			return Post<T>(url, null, e);
		}

		public T Post<T>(string url, object content, HttpRequestArgs e = null)
		{
			return Post<T>(url, CreateContent(content), e);
		}

		public void Post(string url, HttpRequestArgs e = null)
		{
			Post(url, null, e);
		}

		public void Post(string url, object content, HttpRequestArgs e = null)
		{
			var client = e == null || e.Credentials == null
				 ? HttpClientPool.Get(AuthenticationToken, this as IInstanceMetadataProvider)
				 : HttpClientPool.Get(e.Credentials, this as IInstanceMetadataProvider);

			HandleResponse(client.PostAsync(url, CreateContent(content)).GetAwaiter().GetResult(), e);
		}

		public void Post(string url, HttpContent httpContent, HttpRequestArgs e = null)
		{
			var client = e == null || e.Credentials == null
				? HttpClientPool.Get(AuthenticationToken, this as IInstanceMetadataProvider)
				: HttpClientPool.Get(e.Credentials, this as IInstanceMetadataProvider);

			HandleResponse(client.PostAsync(url, httpContent).GetAwaiter().GetResult(), e);
		}

		public T Post<T>(string url, HttpContent httpContent, HttpRequestArgs e)
		{
			var client = e == null || e.Credentials == null
				? HttpClientPool.Get(AuthenticationToken, this as IInstanceMetadataProvider)
				: HttpClientPool.Get(e.Credentials, this as IInstanceMetadataProvider);

			return HandleResponse<T>(client.PostAsync(url, httpContent).GetAwaiter().GetResult(), e);
		}

		private void HandleResponse(HttpResponseMessage response, HttpRequestArgs e)
		{
			if (!response.IsSuccessStatusCode)
				HandleResponseException(response);
		}

		private T HandleResponse<T>(HttpResponseMessage response, HttpRequestArgs e)
		{
			if (!response.IsSuccessStatusCode)
				HandleResponseException(response);

			var content = response.Content.ReadAsStringAsync().Result;

			if (IsNull(content))
				return default(T);

			if (e != null && e.ReadRawResponse)
			{
				if (Types.TryConvert<T>(content, out T result))
					return result;

				return default(T);
			}

			return SerializationExtensions.Deserialize<T>(content);
		}

		private void HandleResponseException(HttpResponseMessage response)
		{
			var rt = string.Empty;

			if (response.Content != null)
				rt = response.Content.ReadAsStringAsync().Result;

			JObject ex = null;

			try
			{
				ex = SerializationExtensions.Deserialize<JObject>(rt);
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

			var c = SerializationExtensions.Serialize(content);

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
