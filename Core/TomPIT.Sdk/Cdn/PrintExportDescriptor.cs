namespace TomPIT.Cdn
{
	public class PrintExportDescriptor : IPrintExportDescriptor
	{
		public byte[] Content { get; set; }

		public string MimeType { get; set; }
	}
}
