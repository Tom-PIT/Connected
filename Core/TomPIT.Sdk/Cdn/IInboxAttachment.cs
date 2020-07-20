namespace TomPIT.Cdn
{
	public interface IInboxAttachment
	{
		string MediaType { get; }
		string ContentType { get; }
		string MediaSubtype { get; }
		string Charset { get; }
		string Name { get; }
		byte[] Content { get; }
	}
}
