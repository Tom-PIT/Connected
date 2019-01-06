namespace TomPIT.Security
{
	public interface IValidationParameters
	{
		string ValidIssuer { get; }
		string ValidAudience { get; }
		string IssuerSigningKey { get; }
	}
}
