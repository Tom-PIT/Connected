namespace TomPIT.Connectivity
{
	internal class BasicCredentials : Credentials, IBasicCredentials
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
