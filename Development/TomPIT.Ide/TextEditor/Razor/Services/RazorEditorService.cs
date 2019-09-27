using Microsoft.CodeAnalysis.Host;

namespace TomPIT.Ide.TextEditor.Razor.Services
{
	internal abstract class RazorEditorService : IWorkspaceService
	{
		public RazorEditorService(RazorEditor editor)
		{
			Editor = editor;
		}

		public RazorEditor Editor { get; }
	}
}
