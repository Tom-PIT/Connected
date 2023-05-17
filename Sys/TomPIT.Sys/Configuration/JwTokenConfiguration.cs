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
			if (!Shell.Configuration.RootElement.TryGetProperty("authentication", out JsonElement element))
				return;

			if (!element.TryGetProperty("jwToken", out JsonElement jwToken))
				return;

			if (jwToken.TryGetProperty("validIssuer", out JsonElement validIssuer))
				ValidIssuer = validIssuer.GetString();

			if (jwToken.TryGetProperty("validAudience", out JsonElement validAudience))
				ValidAudience = validAudience.GetString();

			if (jwToken.TryGetProperty("issuerSigningKey", out JsonElement issuerSigningKey))
				IssuerSigningKey = issuerSigningKey.GetString();
		}
	}
}
