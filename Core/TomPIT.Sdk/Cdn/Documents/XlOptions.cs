namespace TomPIT.Cdn.Documents
{
	public class XlOptions : PageByPageOptions
	{
		private XlEncryptionOptions _encryption = null;
		private XlDocumentOptions _doc = null;
		public XlEncryptionOptions EncryptionOptions { get { return _encryption ??= new XlEncryptionOptions(); } }
		public XlDocumentOptions DocumentOptions { get { return _doc ??= new XlDocumentOptions(); } }
		public string SheetName { get; set; } = "Sheet";
		public DocumentBoolean RightToLeftDocument { get; set; } = DocumentBoolean.Default;
		public bool FitToPrintedPageHeight { get; set; }
		public bool FitToPrintedPageWidth { get; set; }
		public DocumentTextMode TextMode { get; set; } = DocumentTextMode.Value;
		public bool ExportHyperlinks { get; set; } = true;
		public DocumentXlsIgnoreErrors IgnoreErrors { get; set; } = DocumentXlsIgnoreErrors.None;
		public bool RawDataMode { get; set; }
		public bool ShowGridLines { get; set; }
	}
}