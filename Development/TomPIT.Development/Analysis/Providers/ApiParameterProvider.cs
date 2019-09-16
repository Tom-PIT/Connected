using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class ApiParameterProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;
		private Type _argumentsType = null;

		public ApiParameterProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
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

			var descriptor = ComponentDescriptor.Api(context, api);
			var apiRef = descriptor.Component;

			if (apiRef == null)
			{
				apiRef = context.Tenant.GetService<IComponentService>().SelectComponent("Api", descriptor.ComponentName);

				if (apiRef != null && apiRef.MicroService != e.Component.MicroService)
				{
					var ms = context.Tenant.GetService<IMicroServiceService>().Select(e.Component.MicroService);

					try
					{
						var refMicroService = context.Tenant.GetService<IMicroServiceService>().Select(apiRef.MicroService);
						ms.ValidateMicroServiceReference(refMicroService.Name);
					}
					catch
					{
						return null;
					}
				}

				if (apiRef == null)
					return null;
			}

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(apiRef.Token) is IApiConfiguration config))
				return null;

			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (op == null)
				return null;

			var txt = context.Tenant.GetService<IComponentService>().SelectText(apiRef.MicroService, op);

			var parameters = QueryParameters(context, op, txt);

			if (parameters == null || parameters.Count == 0)
				return null;

			var pars = new List<ICodeAnalysisResult>();

			foreach (var i in parameters)
				pars.Add(new CodeAnalysisResult(string.Format("{0} ({1})", i.Name, i.DataType), i.Name, i.Required ? "required" : "optional"));

			var existing = Existing(e.Node as ArgumentSyntax);
			var r = new List<ICodeAnalysisResult>();

			for (int i = pars.Count - 1; i >= 0; i--)
			{
				var p = pars[i];

				if (existing != null && existing.Contains(p.Value.ToLowerInvariant()))
					pars.RemoveAt(i);
			}

			if (pars.Count == 0)
			{
				if (parameters.Count == 0)
				{
					r.Add(new NoSuggestionResult("api has no parameters"));

					return r;
				}
				else if (parameters.Count > 0)
				{
					r.Add(new NoSuggestionResult("all parameters set"));

					return r;
				}
			}

			return pars;
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

		protected override Type ArgumentsType
		{
			get
			{
				if (_argumentsType == null)
					_argumentsType = typeof(ScriptGlobals<>).MakeGenericType(typeof(EventArgs));

				return _argumentsType;
			}
		}

		public override SourceText SourceCode => _sourceCode;
		protected override Guid MicroService => _microService;

		public List<ISuggestion> QueryParameters(IApiOperation operation)
		{
			var txt = Context.Tenant.GetService<IComponentService>().SelectText(operation.Configuration().MicroService(), operation);
			var parameters = QueryParameters(Context, operation, txt);
			var r = new List<ISuggestion>();

			if (parameters == null)
				return r;

			foreach (var parameter in parameters)
			{
				r.Add(new Suggestion
				{
					InsertText = parameter.Name,
					Label = string.Format("{0} ({1})", parameter.Name, parameter.DataType),
					Kind = Suggestion.Property,
					Description = parameter.Required ? "Required" : "Optional"
				});
			}

			return r;
		}

		private List<ApiParameter> QueryParameters(IMiddlewareContext context, IApiOperation operation, string text)
		{
			var schemaParameters = QuerySchemaParameters(context, operation);

			if (schemaParameters != null)
				return schemaParameters;

			if (string.IsNullOrWhiteSpace(text))
				return null;

			_sourceCode = SourceText.From(text);
			_microService = operation.Configuration().MicroService();

			var r = new List<ApiParameter>();
			var o = new List<ICodeAnalysisResult>();

			var model = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var root = model.SyntaxTree.GetRoot();
			var descendants = root.DescendantNodes().Where(f => f is InvocationExpressionSyntax);

			foreach (var i in descendants)
			{
				var si = model.GetSymbolInfo(i);
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
					continue;

				if (type == typeof(JsonExtensions))
				{
					var invoke = i as InvocationExpressionSyntax;

					if (!IsArguments(model, invoke))
						continue;

					if (invoke.ArgumentList.Arguments.Count == 0 || method.TypeArguments.Length == 0)
						continue;

					var arg = invoke.ArgumentList.Arguments[0];
					var argType = method.TypeArguments[0].ToDisplayString();

					var argText = arg.GetText().ToString().Trim('"');

					r.Add(new ApiParameter
					{
						DataType = argType,
						Name = argText,
						Required = string.Compare(method.Name, "Required", false) == 0
					});
				}
			}

			return r;
		}

		public override List<ICodeAnalysisResult> ProvideSnippets(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			ArgumentListSyntax list = null;

			if (e.Node is ArgumentSyntax a)
				list = GetArgumentList(a);
			else if (e.Node is ArgumentListSyntax)
				list = e.Node as ArgumentListSyntax;

			if (list == null)
				return null;

			if (!(list.Parent is InvocationExpressionSyntax invoke))
				return null;

			if (invoke.ArgumentList.Arguments.Count < 1)
				return null;

			var method = GetMethodInfo(e.Model, list);

			if (method == null)
				return null;

			var parameters = method.GetParameters();
			var parameterIndex = -1;

			for (var i = 0; i < parameters.Length; i++)
			{
				if (string.Compare(parameters[i].Name, "api", true) == 0)
				{
					parameterIndex = i;
					break;
				}
			}

			if (parameterIndex == -1)
				return null;

			var tName = invoke.ArgumentList.Arguments[parameterIndex].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(tName))
				return null;

			if (!tName.Contains('/'))
				tName = string.Format("{0}/{1}", e.Component.Name, tName);

			var msName = Context.Tenant.GetService<IMicroServiceService>().Select(e.Component.MicroService);

			var descriptor = ComponentDescriptor.Api(context, $"{msName.Name}/{e.Component.Name}/{tName}");
			var apiRef = context.Tenant.GetService<IComponentService>().SelectComponent(descriptor.MicroService.Token, ComponentCategories.Api, descriptor.ComponentName);

			if (apiRef == null)
			{
				apiRef = context.Tenant.GetService<IComponentService>().SelectComponent(ComponentCategories.Api, descriptor.ComponentName);

				if (apiRef != null && apiRef.MicroService != e.Component.MicroService)
				{
					var ms = context.Tenant.GetService<IMicroServiceService>().Select(e.Component.MicroService);

					try
					{
						var refMs = context.Tenant.GetService<IMicroServiceService>().Select(apiRef.MicroService);

						ms.ValidateMicroServiceReference(refMs.Name);
					}
					catch
					{
						return null;
					}
				}

				if (apiRef == null)
					return null;
			}

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(apiRef.Token) is IApiConfiguration config))
				return null;

			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (op == null)
				return null;

			var txt = context.Tenant.GetService<IComponentService>().SelectText(apiRef.MicroService, op);
			var pars = QueryParameters(context, op, txt);

			if (pars == null || pars.Count == 0)
				return null;

			var r = new List<ICodeAnalysisResult>
			{
				AllParametersSnippet(pars),
				RequiredParametersSnippet(pars)
			};

			return r;
		}

		private CodeAnalysisResult AllParametersSnippet(List<ApiParameter> config)
		{
			var sb = new StringBuilder();

			sb.AppendLine("new JObject{");

			foreach (var i in config.OrderBy(f => f.Name))
				sb.AppendLine(string.Format("\t{{\"{0}\", /*{1}*/}},", i.Name, i.DataType));

			return new CodeAnalysisResult("ap", string.Format("{0}}}", RemoveTrailingComma(sb)), "Insert all api parameters");
		}

		private CodeAnalysisResult RequiredParametersSnippet(List<ApiParameter> config)
		{
			var sb = new StringBuilder();

			sb.AppendLine("new JObject{");

			foreach (var i in config.OrderBy(f => f.Name))
			{
				if (!i.Required)
					continue;

				sb.AppendLine(string.Format("\t{{\"{0}\", /*{1}*/}},", i.Name, i.DataType));
			}

			return new CodeAnalysisResult("rp", string.Format("{0}}}", RemoveTrailingComma(sb)), "Insert required api parameters");
		}

		private bool IsArguments(SemanticModel model, InvocationExpressionSyntax node)
		{
			var idn = GetIdentiferName(node);

			if (idn == null)
				return false;

			if (string.Compare(idn.Identifier.Text, "e", false) != 0)
				return false;

			var ti = model.GetSymbolInfo(idn);

			if (ti.Symbol == null || ti.Symbol.OriginalDefinition == null)
				return false;

			if (string.Compare(ti.Symbol.OriginalDefinition.ToDisplayString(), "TomPIT.Compilation.ScriptGlobals<T>.e", false) != 0)
				return false;

			return true;
		}

		private List<ApiParameter> QuerySchemaParameters(IMiddlewareContext context, IApiOperation operation)
		{
			//var args = new OperationManifestArguments(context, operation);

			//context.Connection().GetService<ICompilerService>().Execute(operation.MicroService(context.Connection()), operation, this, args, out bool handled);

			//if (handled)
			//{
			//    var r = new List<ApiParameter>();

			//    foreach(var i in args.Manifest.Parameters)
			//    {
			//        r.Add(new ApiParameter
			//        {
			//            DataType=i.Type.ToFriendlyName(),
			//            Name=i.Name,
			//            Required=i.IsRequired
			//        });
			//    }

			//    return r;
			//}

			return null;
		}
	}
}
