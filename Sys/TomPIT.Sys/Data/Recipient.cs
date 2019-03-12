using TomPIT.Cdn;

namespace TomPIT.Sys.Data
{
	internal class Recipient : IRecipient
	{
		public SubscriptionResourceType Type { get; set; } = SubscriptionResourceType.User;
		public string ResourcePrimaryKey { get; set; }
	}
}
