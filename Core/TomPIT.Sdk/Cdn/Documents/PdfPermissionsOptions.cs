namespace TomPIT.Cdn.Documents
{
	public enum DocumentPrintingPermissions
	{
		None = 0,
		LowResolution = 1,
		HighResolution = 2
	}

	public enum DocumentChangingPermissions
	{
		None = 0,
		InsertingDeletingRotating = 1,
		FillingSigning = 2,
		CommentingFillingSigning = 3,
		AnyExceptExtractingPages = 4
	}

	public class PdfPermissionsOptions
	{
		public DocumentPrintingPermissions PrintingPermissions { get; set; } = DocumentPrintingPermissions.None;
		public DocumentChangingPermissions ChangingPermissions { get; set; } = DocumentChangingPermissions.None;
		public bool EnableCopying { get; set; }
		public bool EnableScreenReaders { get; set; } = true;
	}
}
