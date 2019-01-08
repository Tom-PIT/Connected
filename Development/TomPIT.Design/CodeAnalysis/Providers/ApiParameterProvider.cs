using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design.Services;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class ApiParameterProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;

		public ApiParameterProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			if (!(e.Node.Parent is ArgumentListSyntax arg))
				return null;

			if (!(arg.Parent is InvocationExpressionSyntax invoke))
				return null;

			if (invoke.ArgumentList.Arguments.Count < 1)
				return null;

			var api = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(api))
				return null;

			if (!api.Contains('/'))
				api = string.Format("{0}/{1}", e.Component.Name, api);

			var q = new ApiQualifier(context, api);
			var apiRef = context.Connection().GetService<IComponentService>().SelectComponent(q.MicroService.Token, "Api", q.Api);

			if (apiRef == null)
			{
				apiRef = context.Connection().GetService<IComponentService>().SelectComponent("Api", q.Api);

				if (apiRef != null && apiRef.MicroService != e.Component.MicroService)
				{
					var ms = context.Connection().GetService<IMicroServiceService>().Select(e.Component.MicroService);

					try
					{
						ms.ValidateMicroServiceReference(context.Connection(), context.Connection().ResolveMicroServiceName(apiRef.MicroService));
					}
					catch
					{
						return null;
					}
				}

				if (api == null)
					return null;
			}

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(apiRef.Token) is IApi config))
				return null;

			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, q.Operation, true) == 0);

			if (op == null)
				return null;

			var txt = context.Connection().GetService<IComponentService>().SelectText(apiRef.MicroService, op.Invoke);

			var parameters = ResolveReferencedApi(context, config, txt);

			if (parameters == null || parameters.Count == 0)
				return null;

			var existing = Existing(e.Node as ArgumentSyntax);
			var r = new List<ICodeAnalysisResult>();

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				var p = parameters[i];

				if (existing != null && existing.Contains(p.Value.ToLowerInvariant()))
					parameters.RemoveAt(i);
			}

			return parameters;
		}

		private List<string> Existing(ArgumentSyntax node)
		{
			var create = node.ChildNodes().FirstOrDefault(f => f is ObjectCreationExpressionSyntax);

			if (create == null)
				return null;

			var initializer = create.ChildNodes().FirstOrDefault(f => f is InitializerExpressionSyntax);

			if (initializer == null)
				return null;

			var r = new List<string>();

			foreach (var i in initializer.ChildNodes())
			{
				var expr = i as InitializerExpressionSyntax;

				if (expr == null)
					continue;

				if (expr.Expressions.Count == 0)
					continue;

				r.Add(expr.Expressions[0].GetText().ToString().Trim('"').ToLowerInvariant());
			}

			if (r.Count == 0)
				return null;

			return r;
		}

		protected override Type ArgumentsType => typeof(OperationInvokeArguments);

		private List<ICodeAnalysisResult> ResolveReferencedApi(IExecutionContext context, IApi api, string text)
		{
			_sourceCode = SourceText.From(text);
			_microService = api.MicroService(Context.Connection());

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

					var argText = arg.GetText().ToString().Trim('"');

					if (string.Compare(method.Name, "Required", false) == 0)
						r.Add(new CodeAnalysisResult(string.Format("{0} ({1})", argText, argType), argText, "required"));
					else if (string.Compare(method.Name, "Optional", false) == 0)
						o.Add(new CodeAnalysisResult(string.Format("{0} ({1})", argText, argType), argText, "optional"));
				}
			}

			if (r.Count > 0 || o.Count > 0)
			{
				var result = new List<ICodeAnalysisResult>();

				r = r.OrderBy(f => f.Text).ToList();

				foreach (var i in r)
					result.Add(i);

				o = o.OrderBy(f => f.Text).ToList();

				foreach (var i in o)
					result.Add(i);

				return result;
			}

			return r;
		}
	}
}
