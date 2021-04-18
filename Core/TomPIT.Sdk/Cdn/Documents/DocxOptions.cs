namespace TomPIT.Cdn.Documents
{
	public enum DocumentDocxMode
	{
		SingleFile = 0,
		SingleFilePageByPage = 1
	}

	public class DocxOptions : FormattedTextOptions
	{
		private DocxDocumentOptions _options = null;
		public DocumentDocxMode Mode { get; set; } = DocumentDocxMode.SingleFilePageByPage;
		public bool TableLayout { get; set; }
		public bool AllowFloatingPictures { get; set; }
		public DocxDocumentOptions DocumentOptions { get { return _options ??= new DocxDocumentOptions(); } }
	}
}
