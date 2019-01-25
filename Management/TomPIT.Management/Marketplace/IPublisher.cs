namespace TomPIT.Marketplace
{
	public enum PublisherStatus
	{
		Inactive = 1,
		Active = 2
	}
	public interface IPublisher
	{
		string Company { get; }
		int Country { get; }
		string Website { get; }
		string Key { get; }
		PublisherStatus Status { get; }
	}
}
