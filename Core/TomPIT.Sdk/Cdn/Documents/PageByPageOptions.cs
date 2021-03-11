namespace TomPIT.Cdn.Documents
{
	public class PageByPageOptions : DocumentCreateOptions
	{
		public string PageRange { get; set; }
		public bool RasterizeImages { get; set; } = true;
		public int RasterizationResolution { get; set; } = 96;
	}
}
