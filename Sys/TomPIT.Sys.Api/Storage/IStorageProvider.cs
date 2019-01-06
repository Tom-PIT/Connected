namespace TomPIT.Api.Storage
{
	public interface IStorageProvider : IClientStorageProvider
	{
		IBlobProvider Blobs { get; }
		IQueueProvider Queue { get; }
		IReliableMessagingProvider Messaging { get; }
	}
}
