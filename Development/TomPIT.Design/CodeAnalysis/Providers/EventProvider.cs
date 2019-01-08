using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class EventProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;

		public EventProvider(IExecutionContext context) : base(context)
		{

		}

		//public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		//{

		//	var de = ResolveComponent(context, context.MicroService(), e.ExpressionText);

		//	if (de == null)
		//		return null;

		//	if (!(context.GetServerContext().GetService<IComponentService>().SelectConfiguration(de.Token) is IDistributedEvent config))
		//		return null;

		//	return ResolveEvent(context, config, txt);
		//}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var existingText = e.ExpressionText ?? string.Empty;
			var ds = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, "Event");
			var r = new List<ICodeAnalysisResult>();

			var msApis = new List<ICodeAnalysisResult>();

			foreach (var i in ds)
			{
				if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(i.Token) is IDistributedEvent config))
					continue;

				var key = i.Name;

				if (i.MicroService != context.MicroService())
				{
					var ms = Context.Connection().GetService<IMicroServiceService>().Select(i.MicroService);

					key = string.Format("{0}/{1}", ms.Name, i.Name);
				}

				msApis.Add(new CodeAnalysisResult(key, key, null));
			}

			msApis.Sort();

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
				var ms = Context.Connection().GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				var ds = context.Connection().GetService<IComponentService>().QueryComponents(context.Connection().ResolveMicroServiceToken(i.MicroService), "Event");

				foreach (var j in ds)
				{
					if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(j.Token) is IEvent config))
						continue;

					var key = string.Format("{0}/{1}", ms.Name, j.Name);

					items.Add(new CodeAnalysisResult(key, key, null));
				}
			}

			items = items.OrderBy(f => f.Text).ToList();

			foreach (var i in items)
				existing.Add(i);
		}

		//private List<ICodeAnalysisResult> ResolveEvent(IExecutionContext context, IApi api, string text)
		//{
		//	var r = new List<ICodeAnalysisResult>();
		//	var o = new List<ICodeAnalysisResult>();

		//	var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
		//		usings: Analyzer.CombineUsings(new List<string> { typeof(OperationInvokeArguments).Namespace }),
		//		metadataReferenceResolver: new MetaDataResolver(context.GetServerContext(), api.MicroService(context.GetServerContext())),
		//		sourceReferenceResolver: new ReferenceResolver(context.GetServerContext(), api.MicroService(context.GetServerContext())));

		//	var t = typeof(ScriptGlobals<>).MakeGenericType(typeof(OperationInvokeArguments));

		//	var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true, hostObjectType: t)
		//		.WithMetadataReferences(Analyzer.CombineReferences(new List<MetadataReference> { MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(OperationInvokeArguments)).Location) }))
		//		.WithCompilationOptions(compilationOptions);

		//	var txt = SourceText.From(text);
		//	var project = Workspace.AddProject(projectInfo);
		//	var docInfo = DocumentInfo.Create(
		//		 DocumentId.CreateNewId(project.Id), "Script",
		//		 sourceCodeKind: SourceCodeKind.Script,
		//		 loader: TextLoader.From(TextAndVersion.Create(txt, VersionStamp.Create())));
		//	var doc = Workspace.AddDocument(docInfo);

		//	var cs = CompletionService.GetService(doc);
		//	var sm = Task.Run(async () => { return await doc.GetSemanticModelAsync(); }).Result;

		//	var root = sm.SyntaxTree.GetRoot();
		//	var descendants = root.DescendantNodes().Where(f => f is InvocationExpressionSyntax);

		//	foreach (var i in descendants)
		//	{
		//		var si = sm.GetSymbolInfo(i);
		//		if (si.Symbol == null)
		//			continue;

		//		var method = si.Symbol as IMethodSymbol;

		//		var declaringTypeName = string.Format(
		//			"{0}.{1}, {2}",
		//			method.ContainingType.ContainingNamespace.ToString(),
		//			method.ContainingType.Name,
		//			method.ContainingAssembly.Name
		//		);

		//		var type = Type.GetType(declaringTypeName);

		//		if (type == null)
		//			return null;

		//		if (type == typeof(JsonExtensions))
		//		{
		//			var invoke = i as InvocationExpressionSyntax;

		//			if (invoke.ArgumentList.Arguments.Count == 0 || method.TypeArguments.Length == 0)
		//				continue;

		//			var arg = invoke.ArgumentList.Arguments[0];
		//			var argType = method.TypeArguments[0].ToDisplayString();

		//			if (string.Compare(method.Name, "Required", false) == 0)
		//				r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1})", arg.GetText().ToString().Trim('"'), argType)), null, null));
		//			else if (string.Compare(method.Name, "Optional", false) == 0)
		//				o.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1}, optional)", arg.GetText().ToString().Trim('"'), argType)), null, null));
		//		}
		//	}

		//	if (r.Count > 0 || o.Count > 0)
		//	{
		//		var result = new List<ICodeAnalysisResult>();

		//		result.Add(new CodeAnalysisResult(ProviderUtils.Header("Parameters"), null, null));

		//		r.Sort();

		//		foreach (var i in r)
		//			result.Add(i);

		//		o.Sort();

		//		foreach (var i in o)
		//			result.Add(i);

		//		return result;
		//	}

		//	return r;
		//}

		public override ICodeLensAnalysisResult CodeLens(IExecutionContext context, CodeAnalysisArgs e)
		{
			var qualifier = e.ExpressionText;

			var ms = context.MicroService();
			var microservice = Context.Connection().GetService<IMicroServiceService>().Select(ms);

			if (qualifier.Contains('/'))
			{
				var msname = qualifier.Split('/')[0];
				qualifier = qualifier.Split('/')[1];

				microservice = Context.Connection().GetService<IMicroServiceService>().Select(msname);

				if (microservice == null)
					return new CodeLensAnalysisResult("Invalid reference", null);

				ms = microservice.Token;
			}

			var ev = ResolveComponent(context, ms, qualifier);

			if (ev == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(ev.Token) is IDistributedEvent config))
				return null;

			return new CodeLensAnalysisResult(ev.Name, string.Format("{0}/{1}", ev.MicroService, ev.Token))
			{
				Command = new CodeLensCommand
				{
					Title = string.Format("{0}/{1}", microservice.Name, qualifier),
					Arguments = new CodeLensArguments
					{
						MicroService = ev.MicroService.ToString(),
						Component = ev.Token.ToString(),
						Element = ev.Token.ToString(),
						Kind = ev.MicroService == context.MicroService() ? CodeLensArguments.InternalLink : CodeLensArguments.ExternalLink
					}
				}
			};
		}

		private IComponent ResolveComponent(IExecutionContext context, Guid microService, string qualifier)
		{
			return context.Connection().GetService<IComponentService>().SelectComponent(microService, "Event", qualifier);
		}
	}
}
