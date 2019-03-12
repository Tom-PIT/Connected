namespace TomPIT.Cdn
{
	public interface IRecipient
	{
		SubscriptionResourceType Type { get; }
		string ResourcePrimaryKey { get; }
	}
}
