using System.Collections.Generic;
using Microsoft.CodeAnalysis.Formatting;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DocumentFormattingEditService : CSharpEditorService, IDocumentFormattingEditService
	{
		public DocumentFormattingEditService(CSharpEditor editor) : base(editor)
		{
		}

		public List<ITextEdit> ProvideDocumentFormattingEdits()
		{
			if (Editor.SemanticModel is null)
				return null;

			var root = Editor.SemanticModel.SyntaxTree.GetRoot();
			var result = Formatter.Format(root, Editor.Workspace);
			var end = Editor.Document.GetLine(root.FullSpan.End);

			return new List<ITextEdit>
			{
				new TextEdit
				{
					Text = result.ToFullString(),
					Range = new Range
					{
						StartColumn = 1,
						StartLineNumber = 1,
						EndColumn = end.ToString().Length + 1,
						EndLineNumber = end.LineNumber + 1
					}
				}
			};
		}
	}
}
