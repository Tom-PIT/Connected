using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	internal class CompilerHoverContext : Analyzer<CodeStateArgs>
	{
		private IHoverInfo _hover = null;

		public CompilerHoverContext(IExecutionContext context, CodeStateArgs e) : base(context, e)
		{
		}

		public IHoverInfo Hover
		{
			get
			{
				if (_hover == null)
					_hover = CreateHover();

				return _hover;
			}
		}

		private IHoverInfo CreateHover()
		{
			var r = new HoverInfo();

			var results = Task.Run(async () =>
			{
				return await Completion.GetCompletionsAsync(Document, Args.Position);
			}).Result;

			if (results == null)
				return CreateContent(Document, Completion.GetDefaultCompletionListSpan(SourceCode, Args.Position));

			return null;
		}

		private IHoverInfo CreateContent(Document doc, TextSpan span)
		{
			var sm = Task.Run(async () => { return await doc.GetSemanticModelAsync(); }).Result;
			var node = sm.SyntaxTree.GetRoot().FindNode(span);

			if (!(node is ArgumentSyntax args))
				return null;

			if (!(args.Expression is LiteralExpressionSyntax expr))
				return null;

			if (!(args.Parent is ArgumentListSyntax list))
				return null;

			if (!(list.Parent is InvocationExpressionSyntax invoke))
				return null;

			var si = sm.GetSymbolInfo(invoke);

			if (si.Symbol == null && si.CandidateSymbols.Length == 0)
				return null;

			IMethodSymbol ms = si.Symbol == null
				? si.CandidateSymbols[0] as IMethodSymbol
				: si.Symbol as IMethodSymbol;

			if (ms == null)
				return null;


			var declaringTypeName = string.Format(
				"{0}.{1}, {2}",
				ms.ContainingType.ContainingNamespace.ToString(),
				ms.ContainingType.Name,
				ms.ContainingAssembly.Name
			);

			var type = Type.GetType(declaringTypeName);

			if (type == null)
				return null;

			var methodName = ms.Name;
			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.ContainingNamespace == null || i.Type.ContainingAssembly == null)
					continue;

				methodArgumentTypeNames.Add(string.Format("{0}.{1}, {2}", i.Type.ContainingNamespace.ToString(), i.Type.Name, i.Type.ContainingAssembly.Name));
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));

			if (argumentTypes.Count() > 0 && argumentTypes.Contains(null))
				return null;

			var methodInfo = Type.GetType(declaringTypeName).GetMethod(methodName, ms.TypeParameters == null ? 0 : ms.TypeParameters.Length, argumentTypes.ToArray());

			if (methodInfo == null)
				return null;

			int index = list.Arguments.IndexOf(args);

			var pars = methodInfo.GetParameters();

			if (pars.Length < index)
				return null;

			var att = pars[index].GetCustomAttribute<CodeAnalysisProviderAttribute>();

			if (att == null)
				return null;

			var provider = att.Type == null
				? Type.GetType(att.TypeName).CreateInstance<ICodeAnalysisProvider>(new object[] { Context })
				: att.Type.CreateInstance<ICodeAnalysisProvider>(new object[] { Context });

			if (provider == null)
				return null;

			var text = ParseExpressionText(expr);
			var items = provider.ProvideHover(Context, new CodeAnalysisArgs(Args.Component, sm, node, si, text));

			if (items == null)
				return null;

			var r = new HoverInfo();

			if (items.Count > 0)
			{
				foreach (var i in items)
					r.Content.Add(new HoverLine { Value = i.Text });
			}

			var sourceRange = node.GetLocation().GetLineSpan();

			r.Range = new Range
			{
				StartColumn = sourceRange.StartLinePosition.Character,
				StartLineNumber = sourceRange.StartLinePosition.Line,
				EndColumn = sourceRange.EndLinePosition.Character,
				EndLineNumber = sourceRange.EndLinePosition.Line
			};

			return r;
		}
	}
}
