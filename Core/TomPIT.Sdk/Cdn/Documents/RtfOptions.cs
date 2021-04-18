namespace TomPIT.Cdn.Documents
{
	public enum DocumentRtfMode
	{
		SingleFile = 0,
		SingleFilePageByPage = 1
	}

	public class RtfOptions : FormattedTextOptions
	{
		public DocumentRtfMode Mode { get; set; } = DocumentRtfMode.SingleFilePageByPage;
	}
}
