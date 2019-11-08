using Microsoft.CodeAnalysis.Host;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal abstract class CSharpEditorService : IWorkspaceService
	{
		public CSharpEditorService(CSharpEditor editor)
		{
			Editor = editor;
		}

		public CSharpEditor Editor { get; }
	}
}
