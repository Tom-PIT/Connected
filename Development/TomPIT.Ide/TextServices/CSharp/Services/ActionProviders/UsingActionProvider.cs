using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.ActionProviders
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
			var span = Editor.Document.GetSpan(new Range
			{
				EndColumn = marker.EndColumn - 1,
				EndLineNumber = marker.EndLineNumber - 1,
				StartColumn = marker.StartColumn - 1,
				StartLineNumber = marker.StartLineNumber - 1
			});

			var result = new List<ICodeAction>();

			if (Arguments.Model.SyntaxTree.Length < span.End)
				return result;

			var node = Arguments.Model.SyntaxTree.GetRoot().FindNode(span);
			var type = CSharpQuery.ResolveTypeInfo(Arguments.Model, node);
			var typeNames = new List<string>();

			if (type.Type != null)
				typeNames.Add(type.Type.MetadataName);

			if (node.IsInAttribute())
			{
				var attributeName = node.EnclosingAttributeName();

				if (!attributeName.EndsWith("Attribute"))
					attributeName += "Attribute";

				if (!typeNames.Contains(attributeName))
					typeNames.Add(attributeName);
			}

			var references = AssemblyReferenceResolver.ResolveReferences(Arguments.Model.Compilation);

			foreach (var typeName in typeNames)
			{
				var types = CSharpReflection.ResolveTypes(references, typeName);

				foreach (var t in types)
				{
					ICodeAction action = null;

					if (string.Compare(t.Name, typeName, false) == 0)
						action = CreateUsing(marker, t.Namespace);
					else
						action = CreateUsingAndChange(marker, t);

					if (result.FirstOrDefault(f => string.Compare(f.Title, action.Title, false) == 0) == null)
						result.Add(action);
				}
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
				Text = $"{title};\n",
				Eol = EndOfLineSequence.CRLF,
				Range = ResolveUsingRange()
			};

			edits.Edits.Add(textEdit);

			return action;
		}

		private ICodeAction CreateUsingAndChange(IMarkerData marker, Type type)
		{
			var result = CreateUsing(marker, type.Namespace);

			var rte = result.Edit as ResourceTextEdit;

			if (rte == null && result.Edit is WorkspaceEdit wsEdit && wsEdit.Edits != null && wsEdit.Edits.Count > 0)
				rte = wsEdit.Edits[0] as ResourceTextEdit;

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
			var end = 1;

			var usings = Arguments.Model.SyntaxTree.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>();

			if (usings.Count() == 0)
			{
				if (Arguments.Model.SyntaxTree.GetRoot().ContainsDirectives)
				{
					var last = Arguments.Model.SyntaxTree.GetRoot().GetLastDirective();

					end = last.FullSpan.End;
				}
			}
			else
				end = usings.Last().FullSpan.End;

			var line = Editor.Document.GetLine(end);

			return new Range
			{
				StartColumn = 1,
				StartLineNumber = line.LineNumber + 1,
				EndColumn = 1,
				EndLineNumber = line.LineNumber + 1
			};
		}
	}
}
