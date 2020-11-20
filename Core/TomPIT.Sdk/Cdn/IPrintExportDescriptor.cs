namespace TomPIT.Cdn
{
	public interface IPrintExportDescriptor
	{
		byte[] Content { get; }
		string MimeType { get; }
	}
}
