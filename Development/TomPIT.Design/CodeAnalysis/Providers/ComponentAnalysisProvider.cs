using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal abstract class ComponentAnalysisProvider : CodeAnalysisProvider
	{
		public ComponentAnalysisProvider(IExecutionContext context) : base(context)
		{

		}

		public override ICodeLensAnalysisResult CodeLens(IExecutionContext context, CodeAnalysisArgs e)
		{
			var c = context.Connection().GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, e.ExpressionText);

			if (c == null)
				return null;

			return new CodeLensAnalysisResult(c.Name, string.Format("{0}/{1}", c.MicroService, c.Token))
			{
				Command = new CodeLensCommand
				{
					Title = string.Format("{0} definition", c.Name),
					Arguments = new CodeLensArguments
					{
						MicroService = c.MicroService.ToString(),
						Component = c.Token.ToString(),
						Element = c.Token.ToString(),
						Kind = c.MicroService == context.MicroService() ? CodeLensArguments.InternalLink : CodeLensArguments.ExternalLink
					}
				}
			};
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var components = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategory);
			var items = new List<ICodeAnalysisResult>();

			foreach (var i in components)
			{
				if (string.IsNullOrWhiteSpace(e.ExpressionText) || i.Name.ToLowerInvariant().Contains(e.ExpressionText.ToLowerInvariant()))
					items.Add(new CodeAnalysisResult(i.Name, i.Name, null));
			}

			return items.OrderBy(f => f.Text).ToList();
		}

		protected abstract string ComponentCategory { get; }

		protected string GetConnection(IExecutionContext context, IComponent component, Guid connection)
		{
			if (connection == Guid.Empty)
				return "?";

			var c = context.Connection().GetService<IComponentService>().SelectComponent(connection);

			if (c == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(c.Token) is IConnection config))
				return null;

			var provider = context.Connection().GetService<IDataProviderService>().Select(config.DataProvider);

			if (provider == null)
				return string.Format("{0} (?)", c.Name);

			return string.Format("{0} ({1})", c.Name, provider.Name);
		}
	}
}
