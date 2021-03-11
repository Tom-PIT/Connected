using TomPIT.Cdn.Documents;

namespace TomPIT.MicroServices.Reporting.Printing
{
	internal class DocumentDescriptor : IDocumentDescriptor
	{
		public byte[] Content {get;set;}

		public string MimeType {get;set;}
	}
}
