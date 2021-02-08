using TomPIT.Cdn.Documents;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareDocuments
	{
		IDocumentDescriptor Create([CIP(CIP.ReportProvider)] string report, DocumentCreateArgs e);
	}
}
