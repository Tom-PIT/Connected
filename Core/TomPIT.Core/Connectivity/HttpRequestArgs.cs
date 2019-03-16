using System;

namespace TomPIT.Connectivity
{
	public class HttpRequestArgs : EventArgs
	{
		public ICredentials Credentials { get; set; }
		public bool ReadRawResponse { get; set; } = false;
	}
}
