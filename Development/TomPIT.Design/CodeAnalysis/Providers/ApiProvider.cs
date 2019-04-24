using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Analysis;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design.Services;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class ApiProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;
		private Type _argumentsType = null;

		public ApiProvider(IExecutionContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
            var r = new List<ICodeAnalysisResult>();
            string existingText = e.ExpressionText ?? string.Empty;

            if (!existingText.Contains('/'))
                existingText = string.Format("{0}/{1}", e.Component.Name, existingText);

            var q = new ApiQualifier(context, existingText);
            var api = ResolveComponent(context, q.MicroService.Token, q.Api);

            if (api == null)
                return null;

            if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(api.Token) is IApi config))
                return null;

            var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, q.Operation, true) == 0);

            if (op == null)
                return null;

            var args = new OperationSchemaArguments(context, op);

            context.Connection().GetService<ICompilerService>().Execute(op.MicroService(context.Connection()), op.Schema, this, args, out bool handled);

            if (handled)
            {
                if (args.Schema.Parameters.Count > 0)
                {
                    r.Add(new CodeAnalysisResult(ProviderUtils.Header("Parameters"), null, null));

                    foreach (var parameter in args.Schema.Parameters)
                    {
                        if (!parameter.IsRequired)
                            r.Add(new CodeAnalysisResult(ProviderUtils.ListItem($"{parameter.Name} ({parameter.Type.ToFriendlyName()})"), null, null));
                        else
                            r.Add(new CodeAnalysisResult(ProviderUtils.ListItem($"{parameter.Name} ({parameter.Type.ToFriendlyName()}, optional)"), null, null));
                    }
                }

                if (args.Schema.ReturnValue != null)
                {
                    r.Add(new CodeAnalysisResult(ProviderUtils.Header(nameof(args.Schema.ReturnValue)), null, null));
                    r.Add(new CodeAnalysisResult($"```json\n{JsonConvert.SerializeObject(args.Schema.ReturnValue, Formatting.Indented)}\n```", null, null));
                }

                return r;
            }
            else
            {
                var txt = context.Connection().GetService<IComponentService>().SelectText(api.MicroService, op.Invoke);

                return ResolveReferencedApi(context, config, txt);
            }
        }

        public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var existingText = e.ExpressionText ?? string.Empty;
			var ds = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, "Api");
			var r = new List<ICodeAnalysisResult>();

			var me = context.Connection().GetService<IComponentService>().SelectConfiguration(e.Component.Token) as IApi;

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
				var config = context.Connection().GetService<IComponentService>().SelectConfiguration(i.Token) as IApi;

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

		private void AddReferences(IExecutionContext context, Guid microService, string existingText, List<ICodeAnalysisResult> existing)
		{
			var items = new List<ICodeAnalysisResult>();
			var refs = context.Connection().GetService<IDiscoveryService>().References(microService);

			if (refs == null || refs.MicroServices.Count == 0)
				return;

			foreach (var i in refs.MicroServices)
			{
				var ms = context.Connection().GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				var ds = context.Connection().GetService<IComponentService>().QueryComponents(context.Connection().ResolveMicroServiceToken(i.MicroService), "Api");

				foreach (var j in ds)
				{
					var config = context.Connection().GetService<IComponentService>().SelectConfiguration(j.Token) as IApi;

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
					_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(typeof(OperationInvokeArguments));

				return _argumentsType;
			}
		}

		public override SourceText SourceCode { get { return _sourceCode; } }

		private List<ICodeAnalysisResult> ResolveReferencedApi(IExecutionContext context, IApi api, string text)
		{
            if (string.IsNullOrWhiteSpace(text))
                return null;

			_sourceCode = SourceText.From(text);
			_microService = api.MicroService(context.Connection());

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

				r.Sort();

				foreach (var i in r)
					result.Add(i);

				o.Sort();

				foreach (var i in o)
					result.Add(i);

				return result;
			}

			return r;
		}

		public override ICodeLensAnalysisResult CodeLens(IExecutionContext context, CodeAnalysisArgs e)
		{
			var qualifier = e.ExpressionText;

			if (!qualifier.Contains('/'))
				qualifier = string.Format("{0}/{1}", e.Component.Name, qualifier);

			var q = new ApiQualifier(context, qualifier);
			var api = ResolveComponent(context, q.MicroService.Token, q.Api);

			if (api == null)
				return null;

			var config = context.Connection().GetService<IComponentService>().SelectConfiguration(api.Token) as IApi;

			if (config == null)
				return null;

			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, q.Operation, true) == 0);

			if (op == null)
				return null;

			return new CodeLensAnalysisResult(api.Name, string.Format("{0}/{1}", api.MicroService, api.Token))
			{
				Command = new CodeLensCommand
				{
					Title = string.Format("{0}/{1}/{2}", q.MicroService.Name, api.Name, op.Name),
					Arguments = new CodeLensArguments
					{
						MicroService = api.MicroService.ToString(),
						Component = api.Token.ToString(),
						Element = op.Id.ToString(),
						Kind = api.MicroService == context.MicroService.Token ? CodeLensArguments.InternalLink : CodeLensArguments.ExternalLink
					}
				}
			};
		}

		private IComponent ResolveComponent(IExecutionContext context, Guid microService, string qualifier)
		{
			var api = context.Connection().GetService<IComponentService>().SelectComponent(microService, "Api", qualifier);

			if (api == null)
			{
				api = context.Connection().GetService<IComponentService>().SelectComponent("Api", qualifier);

				if (api != null && api.MicroService != microService)
				{
					var ms = context.Connection().GetService<IMicroServiceService>().Select(microService);

					try
					{
						ms.ValidateMicroServiceReference(context.Connection(), context.Connection().ResolveMicroServiceName(api.MicroService));
					}
					catch
					{
						return null;
					}
				}
			}

			return api;
		}
	}
}
