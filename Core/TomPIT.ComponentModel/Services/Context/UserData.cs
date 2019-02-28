using Newtonsoft.Json;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	internal class UserData : IUserData
	{
		[JsonProperty("topic")]
		public string Topic { get; set; }
		[JsonProperty("primaryKey")]
		public string PrimaryKey { get; set; }
		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
