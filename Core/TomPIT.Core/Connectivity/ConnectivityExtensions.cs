using System;

namespace TomPIT.Connectivity
{
	public static class ConnectivityExtensions
	{
		public static HttpRequestArgs WithBasicCredentials(this HttpRequestArgs e, string userName, string password)
		{
			e.Credentials = new BasicCredentials
			{
				UserName = userName,
				Password = password
			};

			return e;
		}

		public static HttpRequestArgs WithBearerCredentials(this HttpRequestArgs e, string token)
		{
			e.Credentials = new BearerCredentials
			{
				Token = token
			};

			return e;
		}

		public static HttpRequestArgs WithCurrentCredentials(this HttpRequestArgs e, Guid authenticationToken)
		{
			e.Credentials = new CurrentCredentials
			{
				Token = authenticationToken
			};

			return e;
		}
	}
}
