namespace TomPIT.Api.Storage
{
	public interface IStorageProvider : IClientStorageProvider
	{
		IBlobProvider Blobs { get; }
	}
}
