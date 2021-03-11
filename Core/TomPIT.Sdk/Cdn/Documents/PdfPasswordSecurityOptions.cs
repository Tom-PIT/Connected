namespace TomPIT.Cdn.Documents
{
	public enum DocumentPdfEncryptionLevel
	{
		AES128 = 0,
		AES256 = 1,
		ARC4 = 2
	}

	public class PdfPasswordSecurityOptions
	{
		private PdfPermissionsOptions _options = null;
		public string PermissionsPassword { get; set; }
		public string OpenPassword { get; set; }
		public DocumentPdfEncryptionLevel EncryptionLevel { get; set; } = DocumentPdfEncryptionLevel.AES128;
		public PdfPermissionsOptions PermissionsOptions { get { return _options ??= new PdfPermissionsOptions(); } }
	}
}
