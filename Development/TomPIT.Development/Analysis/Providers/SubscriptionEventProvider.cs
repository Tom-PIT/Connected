using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class SubscriptionEventProvider : ComponentAnalysisProvider
	{
		public SubscriptionEventProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.Subscription;
		protected override bool FullyQualified => true;
		protected override void ProvideComponentLiterals(CodeAnalysisArgs e, List<ICodeAnalysisResult> items, IComponent component)
		{
			if (!(Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is ISubscriptionConfiguration config))
				return;

			foreach (var ev in config.Events)
			{
				if (string.IsNullOrWhiteSpace(ev.Name))
					continue;

				var ms = Context.Tenant.GetService<IMicroServiceService>().Select(ev.Configuration().MicroService());
				var text = $"{ms.Name}/{component.Name}/{ev.Name}";

				items.Add(new CodeAnalysisResult(text, text, null));
			}
		}
	}
}
