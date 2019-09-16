using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.Analysis.Providers
{
	internal abstract class ComponentAnalysisProvider : CodeAnalysisProvider
	{
		public ComponentAnalysisProvider(IMiddlewareContext context) : base(context)
		{

		}

		public override ICodeLensAnalysisResult CodeLens(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var component = e.ExpressionText;

			if (FullyQualified)
			{
				var tokens = component.Split(new char[] { '/' }, 2);

				if (tokens.Length == 2)
					component = tokens[1];
			}

			var c = context.Tenant.GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, component);

			if (c == null)
				return null;

			return new CodeLensAnalysisResult(c.Name, string.Format("{0}/{1}", c.MicroService, c.Token))
			{
				Command = new CodeLensCommand
				{
					Title = $"{c.Name} {ComponentCategory}",
					Arguments = new CodeLensArguments
					{
						MicroService = c.MicroService.ToString(),
						Component = c.Token.ToString(),
						Element = c.Token.ToString(),
						Kind = c.MicroService == context.MicroService.Token ? CodeLensArguments.InternalLink : CodeLensArguments.ExternalLink
					}
				}
			};
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var components = context.Tenant.GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategory);
			var items = new List<ICodeAnalysisResult>();

			foreach (var i in components)
				ProvideComponentLiterals(e, items, i);

			if (IncludeReferences)
			{
				var refs = Context.Tenant.GetService<IDiscoveryService>().References(e.Component.MicroService);

				if (refs != null && refs.MicroServices.Count > 0)
				{
					foreach (var reference in refs.MicroServices)
					{
						var ms = Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);
						components = context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategory);

						foreach (var component in components)
							ProvideComponentLiterals(e, items, component);
					}
				}
			}

			return items.OrderBy(f => f.Text).ToList();
		}

		protected virtual void ProvideComponentLiterals(CodeAnalysisArgs e, List<ICodeAnalysisResult> items, IComponent component)
		{
			if (FullyQualified)
			{
				var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

				items.Add(new CodeAnalysisResult($"{ms.Name}/{component.Name}", $"{ms.Name}/{component.Name}", null));
			}
			else
				items.Add(new CodeAnalysisResult(component.Name, component.Name, null));
		}

		protected abstract string ComponentCategory { get; }
		protected abstract bool FullyQualified { get; }
		protected virtual bool IncludeReferences => false;

		protected string GetConnection(IMiddlewareContext context, IComponent component, Guid connection)
		{
			if (connection == Guid.Empty)
				return "?";

			var c = context.Tenant.GetService<IComponentService>().SelectComponent(connection);

			if (c == null)
				return null;

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(c.Token) is IConnectionConfiguration config))
				return null;

			var provider = context.Tenant.GetService<IDataProviderService>().Select(config.DataProvider);

			if (provider == null)
				return string.Format("{0} (?)", c.Name);

			return string.Format("{0} ({1})", c.Name, provider.Name);
		}
	}
}
