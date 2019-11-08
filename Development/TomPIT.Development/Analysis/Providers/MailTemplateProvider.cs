using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class MailTemplateProvider : ComponentAnalysisProvider
	{
		public MailTemplateProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.MailTemplate;
		protected override bool FullyQualified => true;
	}
}
