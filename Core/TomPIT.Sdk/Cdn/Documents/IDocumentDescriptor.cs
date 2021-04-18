namespace TomPIT.Cdn.Documents
{
	public interface IDocumentDescriptor
	{
		byte[] Content { get; }
		string MimeType { get; }
	}
}
