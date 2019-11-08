using Microsoft.CodeAnalysis.Host;

namespace TomPIT.Ide.TextServices.Razor.Services
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
