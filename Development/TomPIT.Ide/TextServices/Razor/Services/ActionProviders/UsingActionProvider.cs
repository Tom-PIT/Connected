using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Razor.Services.ActionProviders
{
	internal class UsingActionProvider : ActionProvider
	{
		protected override List<ICodeAction> OnGetActions()
		{
			if (Arguments.Context.Markers.Count == 0)
				return null;

			var result = new List<ICodeAction>();

			foreach (var marker in Arguments.Context.Markers)
			{
				if (string.Compare(marker.Code, "CS0246", true) == 0
					|| string.Compare(marker.Code, "CS0103", true) == 0)
				{
					var results = GetUsingActions(marker);

					if (results != null && results.Count > 0)
						result.AddRange(results);
				}
				else if (string.Compare(marker.Code, "CS1061", true) == 0)
				{
					var results = GetUsingActionsFromDefinition(marker);

					if (results != null && results.Count > 0)
						result.AddRange(results);
				}
			}

			return result;
		}

		private List<ICodeAction> GetUsingActionsFromDefinition(IMarkerData marker)
		{
			var methodName = Arguments.Node.FirstAncestorOrSelf<IdentifierNameSyntax>();

			if (methodName == null)
				return null;

			var typeInfo = CSharpQuery.ResolveMemberAccessTypeInfo(Arguments.Model, Arguments.Node);

			if (typeInfo.Type == null)
				return null;

			var references = AssemblyReferenceResolver.ResolveReferences(Arguments.Model.Compilation);
			var methods = CSharpReflection.GetExtensionMethods(references, methodName.Identifier.ValueText).Select(f => f.DeclaringType.Namespace).Distinct();
			var result = new List<ICodeAction>();

			foreach (var method in methods)
				result.Add(CreateUsing(marker, method));

			return result;
		}
		private List<ICodeAction> GetUsingActions(IMarkerData marker)
		{
			var type = CSharpQuery.ResolveTypeInfo(Arguments.Model, Arguments.Node);

			if (type.Type == null)
				return null;

			var typeName = type.Type.MetadataName;
			var references = AssemblyReferenceResolver.ResolveReferences(Arguments.Model.Compilation);
			var types = CSharpReflection.ResolveTypes(references, typeName);
			var result = new List<ICodeAction>();

			foreach (var t in types)
			{
				if (string.Compare(t.Name, typeName, false) == 0)
					result.Add(CreateUsing(marker, t.Namespace));
				else
					result.Add(CreateUsingAndChange(marker, t));
			}

			return result;
		}

		private ICodeAction CreateUsing(IMarkerData marker, string @namespace)
		{
			var title = $"using {@namespace}";

			var edits = new WorkspaceEdit();
			var action = new CodeAction
			{
				Title = title,
				IsPreferred = true,
				Edit = edits,
				Kind = "quickfix"
			};

			action.Diagnostics.Add(marker);

			var textEdit = new ResourceTextEdit
			{
				Resource = Editor.Model.Uri,
				ModelVersionId = Editor.Model.Version
			};

			textEdit.Edit = new TextEdit
			{
				Text = $"@{title};\n",
				Eol = EndOfLineSequence.CRLF,
				Range = ResolveUsingRange()
			};

			edits.Edits.Add(textEdit);

			return action;
		}

		private ICodeAction CreateUsingAndChange(IMarkerData marker, Type type)
		{
			var result = CreateUsing(marker, type.Namespace);

			var rte = result.Edit.Edits[0] as ResourceTextEdit;

			rte.Edit = new TextEdit
			{
				Text = type.Name,
				Range = new Range
				{
					StartColumn = marker.StartColumn,
					EndColumn = marker.EndColumn,
					EndLineNumber = marker.EndLineNumber,
					StartLineNumber = marker.StartLineNumber
				}
			};

			return result;
		}
		private Range ResolveUsingRange()
		{
			LinePosition end = new LinePosition(1, 1);

			var us = LastUsingDirective();

			if (us != null)
				end = us.GetLocation().GetMappedLineSpan().EndLinePosition;

			return new Range
			{
				StartColumn = end.Character,
				StartLineNumber = end.Line + 2,
				EndColumn = end.Character,
				EndLineNumber = end.Line + 2
			};
		}

		private UsingDirectiveSyntax LastUsingDirective()
		{
			var usings = Arguments.Model.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

			if (usings.Count == 0)
				return null;

			UsingDirectiveSyntax last = null;

			foreach (var u in usings)
			{
				if (!u.GetLocation().GetMappedLineSpan().HasMappedPath || string.Compare(u.GetLocation().GetMappedLineSpan().Path, $"{Editor.Model.Id}.cshtml") != 0)
					continue;

				last = u;
			}

			return last;
		}
	}
}
