using TomPIT.Security;

namespace TomPIT.Sys.Model.Security
{
	internal class UserDataItem : IUserData
	{
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Value { get; set; }
	}
}
