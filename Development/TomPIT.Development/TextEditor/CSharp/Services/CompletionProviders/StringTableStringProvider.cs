using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class StringTableStringProvider : CompletionProvider
	{
		private Guid _microService = Guid.Empty;

		protected override List<ICompletionItem> OnProvideItems()
		{
			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(Editor.GetMappedSpan(Arguments.Position));
			var stringTable = string.Empty;

			if (node.Parent is ArgumentListSyntax arg)
			{
				if (!(arg.Parent is InvocationExpressionSyntax invoke))
					return null;

				if (invoke.ArgumentList.Arguments.Count < 1)
					return null;

				stringTable = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');
			}
			else if (node.Parent is AttributeArgumentListSyntax aarg)
			{
				if (!(aarg.Parent is AttributeSyntax invoke))
					return null;

				if (invoke.ArgumentList.Arguments.Count < 1)
					return null;

				stringTable = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');
			}
			else
				return null;

			if (string.IsNullOrWhiteSpace(stringTable))
				return null;

			var microService = Editor.Context.MicroService;

			if (stringTable.Contains('/'))
			{
				var tokens = stringTable.Split('/');

				Editor.Context.MicroService.ValidateMicroServiceReference(tokens[0]);

				microService = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new Exception($"{SR.ErrMicroServiceNotFound} ({microService})");

				stringTable = tokens[1];
			}

			var ds = Editor.Context.Tenant.GetService<IComponentService>().SelectComponent(microService.Token, ComponentCategories.StringTable, stringTable);

			if (ds == null)
				return null;

			if (!(Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(ds.Token) is IStringTableConfiguration config))
				return null;

			var r = new List<ICompletionItem>();

			if (config.Strings.Count == 0)
			{
				r.Add(new CompletionItem
				{
					Label = "string table contains not strings",
					Kind = CompletionItemKind.Value
				});

				return r;
			}

			foreach (var i in config.Strings)
			{
				r.Add(new CompletionItem
				{
					Detail = Types.EllipseString(i.DefaultValue, 256),
					Kind = CompletionItemKind.Text,
					Label = i.Key,
					FilterText = $"{i.Key}{Types.EllipseString(i.DefaultValue, 256)}",
					InsertText = i.Key,
					SortText = i.Key
				});
			}

			return r;
		}

		//public override List<ICodeAnalysisResult> ProvideHover(IMiddlewareContext context, CodeAnalysisArgs e)
		//{
		//	if (!(e.Node is ArgumentSyntax arg))
		//		return null;

		//	var list = e.Node.Parent as ArgumentListSyntax;

		//	var table = list.Arguments[0].GetText().ToString();
		//	var str = e.Node.GetText().ToString();

		//	if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(str))
		//		return null;

		//	var value = context.Services.Globalization.GetString(table.Substring(1, table.Length - 2), str.Substring(1, str.Length - 2));

		//	if (string.IsNullOrWhiteSpace(value))
		//		return null;

		//	var r = new List<ICodeAnalysisResult>
		//	{
		//		new CodeAnalysisResult( value)
		//	};

		//	return r;
		//}
	}
}
