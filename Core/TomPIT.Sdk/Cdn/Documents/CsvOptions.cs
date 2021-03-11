using System.Globalization;

namespace TomPIT.Cdn.Documents
{
	public class CsvOptions : TextOptions
	{
		public CultureInfo DocumentCulture { get; set; } = CultureInfo.CurrentUICulture;

		public bool WritePreamble { get; set; }

		public bool SuppressEmptyStrings { get; set; }

		public DocumentType Type { get; set; } = DocumentType.DataAware;

		public bool SkipEmptyRows { get; set; } = true;

		public bool SkipEmptyColumns { get; set; } = true;

		public bool UseCustomSeparator { get; set; }

		public DocumentBoolean EncodeExecutableContent { get; set; } = DocumentBoolean.Default;
	}
}
