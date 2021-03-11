using System.Drawing;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentHtmlMode
	{
		SingleFile = 0,
		SingleFilePageByPage = 1,
		DifferentFiles = 2
	}

	public class HtmlOptions : PageByPageOptions
	{
		public bool UseHRefHyperlinks { get; set; }
		public bool InlineCss { get; set; }
		public bool Watermarks { get; set; } = true;
		public bool TableLayout { get; set; } = true;
		public bool EmbedImagesInHTML { get; set; }
		public bool RemoveSecondarySymbols { get; set; }
		public string Title { get; set; } = "Document";
		public bool AllowURLsWithJSContent { get; set; }
		public DocumentHtmlMode Mode { get; set; } = DocumentHtmlMode.SingleFile;
		public int PageBorderWidth { get; set; } = 1;
		public Color PageBorderColor { get; set; }
		public string CharacterSet { get; set; } = "utf-8";
	}
}