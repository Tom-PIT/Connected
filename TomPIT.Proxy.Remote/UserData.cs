using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class UserData : IUserData
	{
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Value { get; set; }
	}
}
