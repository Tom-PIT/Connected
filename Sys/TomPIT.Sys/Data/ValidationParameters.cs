using TomPIT.Security;

namespace TomPIT.Sys.Data
{
	internal class ValidationParameters : IValidationParameters
	{
		public string ValidIssuer { get; set; }
		public string ValidAudience { get; set; }
		public string IssuerSigningKey { get; set; }
	}
}
