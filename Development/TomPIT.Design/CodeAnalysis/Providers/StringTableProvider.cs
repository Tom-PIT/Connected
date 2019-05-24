using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class StringTableProvider : ComponentAnalysisProvider
	{
		public StringTableProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "StringTable";

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var components = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategory);
			var items = new List<ICodeAnalysisResult>();

			foreach (var component in components)
				items.Add(new CodeAnalysisResult(component.Name, component.Name, null));

			items = items.OrderBy(f => f.Text).ToList();

			var refs = context.Connection().GetService<IDiscoveryService>().References(e.Component.MicroService);

			if (refs == null || refs.MicroServices.Count == 0)
				return items;

			foreach(var reference in refs.MicroServices)
			{
				var ms = context.Connection().GetService<IMicroServiceService>().Select(reference.MicroService);

				components = context.Connection().GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategory);

				foreach (var component in components)
					items.Add(new CodeAnalysisResult($"{ms.Name}/{component.Name}", $"{ms.Name}/{component.Name}", null));
			}

			return items;
		}
	}
}
