using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.Analysis.Providers
{
	internal class PartialProvider : ComponentAnalysisProvider
	{
		public PartialProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.Partial;
		protected override bool FullyQualified => false;
		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var components = context.Tenant.GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategory);
			var items = new List<ICodeAnalysisResult>();

			foreach (var component in components)
			{
				var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);
				var value = $"{ms.Name}/{component.Name}";

				items.Add(new CodeAnalysisResult(value, value, null));
			}

			items = items.OrderBy(f => f.Text).ToList();

			var refs = context.Tenant.GetService<IDiscoveryService>().References(e.Component.MicroService);

			if (refs == null || refs.MicroServices.Count == 0)
				return items;

			foreach (var reference in refs.MicroServices)
			{
				var ms = context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

				components = context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategory);

				foreach (var component in components)
					items.Add(new CodeAnalysisResult($"{ms.Name}/{component.Name}", $"{ms.Name}/{component.Name}", null));
			}

			return items;
		}
	}
}
