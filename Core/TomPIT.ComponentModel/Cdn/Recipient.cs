namespace TomPIT.Cdn
{
	public class Recipient : IRecipient
	{
		public SubscriptionResourceType Type { get; set; } = SubscriptionResourceType.User;
		public string ResourcePrimaryKey { get; set; }
	}
}
