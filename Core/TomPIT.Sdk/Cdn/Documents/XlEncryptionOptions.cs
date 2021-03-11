namespace TomPIT.Cdn.Documents
{
	public enum DocumentXlsEncryptionType
	{
		Compatible = 1,
		Strong = 2
	}

	public class XlEncryptionOptions
	{
		public DocumentXlsEncryptionType Type { get; set; } = DocumentXlsEncryptionType.Strong;
		public string Password { get; set; }
	}
}
