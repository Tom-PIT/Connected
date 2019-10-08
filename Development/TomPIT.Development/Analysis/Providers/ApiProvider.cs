using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Lenses;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.Analysis.Providers
{
	internal class ApiProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;
		private Type _argumentsType = null;

		public ApiProvider(IMiddlewareContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideHover(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var r = new List<ICodeAnalysisResult>();
			string existingText = e.ExpressionText ?? string.Empty;

			if (!existingText.Contains('/'))
				existingText = string.Format("{0}/{1}", e.Component.Name, existingText);

			var descriptor = ComponentDescriptor.Api(context, existingText);

			if (descriptor.Configuration == null)
				return null;

			var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (op == null)
				return null;

			var txt = context.Tenant.GetService<IComponentService>().SelectText(descriptor.MicroService.Token, op);

			return ResolveReferencedApi(context, descriptor.Configuration, txt);
		}
		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var existingText = e.ExpressionText ?? string.Empty;
			var ds = context.Tenant.GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategories.Api);
			var r = new List<ICodeAnalysisResult>();

			var me = context.Tenant.GetService<IComponentService>().SelectConfiguration(e.Component.Token) as IApiConfiguration;

			if (me != null)
			{
				foreach (var i in me.Operations.OrderBy(f => f.Name))
				{
					if (string.IsNullOrWhiteSpace(i.Name))
						continue;

					var key = i.Name;

					if (string.IsNullOrWhiteSpace(existingText) || key.ToLowerInvariant().Contains(existingText.ToLowerInvariant()))
						r.Add(new CodeAnalysisResult(key, key, string.Empty));
				}
			}

			var msApis = new List<ICodeAnalysisResult>();

			foreach (var i in ds)
			{
				var config = context.Tenant.GetService<IComponentService>().SelectConfiguration(i.Token) as IApiConfiguration;

				if (config == null || config.Component == e.Component.Token)
					continue;

				foreach (var j in config.Operations)
				{
					if (j.Scope == ElementScope.Private)
						continue;

					var key = string.Format("{0}/{1}", i.Name, j.Name);

					if (string.IsNullOrWhiteSpace(existingText) || key.ToLowerInvariant().Contains(existingText.ToLowerInvariant()))
						msApis.Add(new CodeAnalysisResult(key, key, null));
				}
			}

			msApis = msApis.OrderBy(f => f.Text).ToList();

			foreach (var i in msApis)
				r.Add(i);

			AddReferences(context, e.Component.MicroService, existingText, r);

			return r;
		}

		private void AddReferences(IMiddlewareContext context, Guid microService, string existingText, List<ICodeAnalysisResult> existing)
		{
			var items = new List<ICodeAnalysisResult>();
			var refs = context.Tenant.GetService<IDiscoveryService>().References(microService);

			if (refs == null || refs.MicroServices.Count == 0)
				return;

			foreach (var i in refs.MicroServices)
			{
				var ms = context.Tenant.GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;
				var msName = context.Tenant.GetService<IMicroServiceService>().Select(i.MicroService);
				var ds = context.Tenant.GetService<IComponentService>().QueryComponents(msName.Token, ComponentCategories.Api);

				foreach (var j in ds)
				{
					var config = context.Tenant.GetService<IComponentService>().SelectConfiguration(j.Token) as IApiConfiguration;

					if (config == null)
						continue;

					if (config.Scope != ElementScope.Public)
						continue;

					foreach (var k in config.Operations)
					{
						if (k.Scope != ElementScope.Public)
							continue;

						var key = string.Format("{0}/{1}/{2}", ms.Name, j.Name, k.Name);

						if (string.IsNullOrWhiteSpace(existingText) || key.ToLowerInvariant().Contains(existingText.ToLowerInvariant()))
							items.Add(new CodeAnalysisResult(key, key, null));
					}
				}
			}

			items = items.OrderBy(f => f.Text).ToList();

			foreach (var i in items)
				existing.Add(i);
		}

		protected override Type ArgumentsType
		{
			get
			{
				if (_argumentsType == null)
					_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(typeof(EventArgs));

				return _argumentsType;
			}
		}

		public override SourceText SourceCode { get { return _sourceCode; } }

		private List<ICodeAnalysisResult> ResolveReferencedApi(IMiddlewareContext context, IApiConfiguration api, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			_sourceCode = SourceText.From(text);
			_microService = api.MicroService();

			var r = new List<ICodeAnalysisResult>();
			var o = new List<ICodeAnalysisResult>();

			var sm = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var root = sm.SyntaxTree.GetRoot();
			var descendants = root.DescendantNodes().Where(f => f is InvocationExpressionSyntax);

			foreach (var i in descendants)
			{
				var si = sm.GetSymbolInfo(i);
				if (si.Symbol == null)
					continue;

				var method = si.Symbol as IMethodSymbol;

				var declaringTypeName = string.Format(
					"{0}.{1}, {2}",
					method.ContainingType.ContainingNamespace.ToString(),
					method.ContainingType.Name,
					method.ContainingAssembly.Name
				);

				var type = Type.GetType(declaringTypeName);

				if (type == null)
					return null;

				if (type == typeof(JsonExtensions))
				{
					var invoke = i as InvocationExpressionSyntax;

					if (invoke.ArgumentList.Arguments.Count == 0 || method.TypeArguments.Length == 0)
						continue;

					var arg = invoke.ArgumentList.Arguments[0];
					var argType = method.TypeArguments[0].ToDisplayString();

					if (string.Compare(method.Name, "Required", false) == 0)
						r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1})", arg.GetText().ToString().Trim('"'), argType)), null, null));
					else if (string.Compare(method.Name, "Optional", false) == 0)
						o.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1}, optional)", arg.GetText().ToString().Trim('"'), argType)), null, null));
				}
			}

			if (r.Count > 0 || o.Count > 0)
			{
				var result = new List<ICodeAnalysisResult>();

				result.Add(new CodeAnalysisResult(ProviderUtils.Header("Parameters"), null, null));

				//r.Sort();

				foreach (var i in r)
					result.Add(i);

				//o.Sort();

				foreach (var i in o)
					result.Add(i);

				return result;
			}

			return r;
		}

		public override ICodeLensAnalysisResult CodeLens(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return default;
			//var qualifier = e.ExpressionText;

			//if (!qualifier.Contains('/'))
			//	qualifier = string.Format("{0}/{1}", e.Component.Name, qualifier);

			//var descriptor = ComponentDescriptor.Api(context, qualifier);

			//if (descriptor.Configuration == null)
			//	return null;

			//var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			//if (op == null)
			//	return null;

			//return new CodeLensAnalysisResult(descriptor.ComponentName, string.Format("{0}/{1}", descriptor.MicroService.Token, descriptor.Component.Token))
			//{
			//	Command = new CodeLensCommand
			//	{
			//		Title = string.Format("{0}/{1}/{2}", descriptor.MicroService.Name, descriptor.ComponentName, descriptor.Element),
			//		Arguments = new CodeLensArguments
			//		{
			//			MicroService = descriptor.MicroService.Token.ToString(),
			//			Component = descriptor.Component.Token.ToString(),
			//			Element = op.Id.ToString(),
			//			Kind = descriptor.MicroService.Token == context.MicroService.Token ? CodeLensArguments.InternalLink : CodeLensArguments.ExternalLink
			//		}
			//	}
			//};
		}

		private IComponent ResolveComponent(IMiddlewareContext context, Guid microService, string qualifier)
		{
			var api = context.Tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.Api, qualifier);

			//if (api == null)
			//{
			//	api = context.Tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.Api, qualifier);

			//	if (api != null && api.MicroService != microService)
			//	{
			//		var ms = context.Tenant.GetService<IMicroServiceService>().Select(microService);

			//		try
			//		{
			//			var msName = context.Tenant.GetService<IMicroServiceService>().Select(api.MicroService);
			//			ms.ValidateMicroServiceReference(msName.Name);
			//		}
			//		catch
			//		{
			//			return null;
			//		}
			//	}
			//}

			return api;
		}
	}
}
