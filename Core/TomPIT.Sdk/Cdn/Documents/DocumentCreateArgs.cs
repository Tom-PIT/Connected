namespace TomPIT.Cdn.Documents
{
	public class DocumentCreateArgs
	{
		private DocumentCreateOptions _options = null;
		public DocumentCreateOptions Options
		{
			get
			{
				if(_options == null)
				{
					switch (Format)
					{
						case DocumentFormat.Csv:
							_options = new CsvOptions();
							break;
						case DocumentFormat.Docx:
							_options = new DocxOptions();
							break;
						case DocumentFormat.Html:
							_options = new HtmlOptions();
							break;
						case DocumentFormat.Image:
							_options = new ImageOptions();
							break;
						case DocumentFormat.Mht:
							_options = new MhtOptions();
							break;
						case DocumentFormat.Pdf:
							_options = new PdfOptions();
							break;
						case DocumentFormat.Rtf:
							_options = new RtfOptions();
							break;
						case DocumentFormat.Text:
							_options = new TextOptions();
							break;
						case DocumentFormat.Xls:
							_options = new XlsOptions();
							break;
						case DocumentFormat.Xlsx:
							_options = new XlsxOptions();
							break;
					}
				}

				return _options;
			}
			set
			{
				_options = value;
			}
		}
		public string Provider { get; }
		public DocumentFormat Format { get; set; } = DocumentFormat.Pdf;
		public string User { get; set; }
		public object Arguments { get; set; }
	}
}
