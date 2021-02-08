using TomPIT.Cdn;
using TomPIT.Cdn.Documents;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareDocuments : MiddlewareObject, IMiddlewareDocuments
	{
		public MiddlewareDocuments(IMiddlewareContext context) : base(context)
		{

		}

		public IDocumentDescriptor Create([CIP(CIP.ReportProvider)] string report, DocumentCreateArgs e)
		{
			return Context.Tenant.GetService<IDocumentService>().Create(report, e);
		}
	}
}
