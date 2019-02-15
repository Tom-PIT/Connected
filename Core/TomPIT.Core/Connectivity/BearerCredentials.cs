namespace TomPIT.Connectivity
{
	internal class BearerCredentials : Credentials, IBearerCredentials
	{
		public string Token { get; set; }
	}
}
