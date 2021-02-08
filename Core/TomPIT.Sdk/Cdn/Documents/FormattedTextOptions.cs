namespace TomPIT.Cdn.Documents
{
	public class FormattedTextOptions : PageByPageOptions
	{
		public bool Watermarks { get; set; } = true;
		public bool PageBreaks { get; set; } = true;
		public bool EmptyFirstPageHeaderFooter { get; set; }
		public bool KeepRowHeight { get; set; }
	}
}