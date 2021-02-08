using System;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentPdfAttachmentRelationship
	{
		Alternative = 0,
		Data = 1,
		Source = 2,
		Supplement = 3,
		Unspecified = 4
	}

	public class PdfAttachment
	{
		public DateTime? CreationDate { get; set; }
		public DateTime? ModificationDate { get; set; }
		public string Type { get; set; }
		public DocumentPdfAttachmentRelationship Relationship { get; set; }
		public byte[] Data { get; set; }
		public string FileName { get; set; }
		public string Description { get; set; }
		public string FilePath { get; set; }
	}
}
