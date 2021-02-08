using System.Collections.Generic;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentPdfACompatibility
	{
		None = 0,
		PdfA1b = 1,
		PdfA2b = 2,
		PdfA3b = 3
	}

	public enum DocumentPdfJpegImageQuality
	{
		Lowest = 10,
		Low = 25,
		Medium = 50,
		High = 75,
		Highest = 100
	}

	public class PdfOptions : PageByPageOptions
	{
		private PdfSignatureOptions _signature = null;
		private PdfPasswordSecurityOptions _security = null;
		private PdfDocumentOptions _doc = null;
		private List<PdfAttachment> _attachments = null;

		public DocumentPdfACompatibility PdfACompatibility { get; set; } = DocumentPdfACompatibility.None;
		public PdfSignatureOptions SignatureOptions { get { return _signature ??= new PdfSignatureOptions(); } }
		public DocumentPdfJpegImageQuality ImageQuality { get; set; } = DocumentPdfJpegImageQuality.Highest;
		public string NeverEmbeddedFonts { get; set; }
		public PdfPasswordSecurityOptions PasswordSecurityOptions { get { return _security ??= new PdfPasswordSecurityOptions(); } }
		public PdfDocumentOptions DocumentOptions { get { return _doc ??= new PdfDocumentOptions(); } }
		public bool ShowPrintDialogOnOpen { get; set; }
		public bool ExportEditingFieldsToAcroForms { get; set; }
		public bool ConvertImagesToJpeg { get; set; } = true;
		public string AdditionalMetadata { get; set; }
		public List<PdfAttachment> Attachments { get { return _attachments ??= new List<PdfAttachment>(); } }
	}
}
