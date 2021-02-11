using TomPIT.Security;

namespace TomPIT.Sys.Model
{
	internal class ValidationParameters : IValidationParameters
	{
		public string ValidIssuer { get; set; }
		public string ValidAudience { get; set; }
		public string IssuerSigningKey { get; set; }
	}
}
