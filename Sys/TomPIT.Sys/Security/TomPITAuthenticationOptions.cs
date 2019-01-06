using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace TomPIT.Sys.Security
{
	public class TomPITAuthenticationOptions : AuthenticationSchemeOptions
	{
		private static Lazy<Dictionary<string, string>> _clientKeys = new Lazy<Dictionary<string, string>>();

		public static Dictionary<string, string> ClientKeys
		{
			get { return _clientKeys.Value; }
		}
	}
}
