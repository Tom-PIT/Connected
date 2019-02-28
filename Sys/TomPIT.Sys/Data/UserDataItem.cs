using TomPIT.Security;

namespace TomPIT.Sys.Data
{
	internal class UserDataItem : IUserData
	{
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Value { get; set; }
	}
}
