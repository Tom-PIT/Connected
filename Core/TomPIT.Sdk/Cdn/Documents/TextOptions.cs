using System.Text;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentTextMode
	{
		Value = 0,
		Text = 1
	}

	public class TextOptions : DocumentCreateOptions
	{
		public bool QuoteStringsWithSeparators { get; set; }

		public Encoding Encoding { get; set; } = Encoding.UTF8;

		public DocumentTextMode TextMode { get; set; } = DocumentTextMode.Text;

		public string Separator { get; set; } = ";";
	}
}
