﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class SearchCatalogProvider : ComponentAnalysisProvider
	{
		public SearchCatalogProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "SearchCatalog";
		protected override bool FullyQualified => false;
		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var components = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategory);
			var items = new List<ICodeAnalysisResult>();

			foreach (var component in components)
			{
				var ms = Context.Connection().GetService<IMicroServiceService>().Select(component.MicroService);
				var value = $"{ms.Name}/{component.Name}";

				items.Add(new CodeAnalysisResult(value, value, null));
			}

			items = items.OrderBy(f => f.Text).ToList();

			var refs = context.Connection().GetService<IDiscoveryService>().References(e.Component.MicroService);

			if (refs == null || refs.MicroServices.Count == 0)
				return items;

			foreach (var reference in refs.MicroServices)
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