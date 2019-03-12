namespace TomPIT.SysDb.Cdn
{
	public interface ICdnHandler
	{
		IMailHandler Mail { get; }
		ISubscriptionHandler Subscription { get; }
	}
}
