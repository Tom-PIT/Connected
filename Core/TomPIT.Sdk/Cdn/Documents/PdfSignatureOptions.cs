using System.Security.Cryptography.X509Certificates;

namespace TomPIT.Cdn.Documents
{
	public class PdfSignatureOptions
	{
		public string Reason { get; set; }
		public string Location { get; set; }
		public string ContactInfo { get; set; }
		public X509Certificate2 Certificate { get; set; }
	}
}
