using TomPIT.Cdn.Documents;

namespace TomPIT.Cdn
{
	public enum DocumentFormat
	{
		Csv = 1,
		Docx = 2,
		Html = 3,
		Image = 4,
		Mht = 5,
		Pdf = 6,
		Rtf = 7,
		Text = 8,
		Xls = 9,
		Xlsx = 10,
	}

	public interface IDocumentService
	{
		IDocumentDescriptor Create(string report, DocumentCreateArgs e);
		IDocumentProvider GetProvider(string name);
	}
}
