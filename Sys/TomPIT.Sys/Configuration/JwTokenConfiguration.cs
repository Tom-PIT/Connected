using Microsoft.Extensions.Configuration;

using System.Text.Json;

namespace TomPIT.Sys.Configuration
{
	internal class JwTokenConfiguration
	{
		public JwTokenConfiguration()
		{
			Initialize();
		}

		public string ValidIssuer { get; set; }
		public string ValidAudience { get; set; }
		public string IssuerSigningKey { get; set; }

		private void Initialize()
		{
			Shell.Configuration.Bind("authentication:jwToken", this);
		}
	}
}
