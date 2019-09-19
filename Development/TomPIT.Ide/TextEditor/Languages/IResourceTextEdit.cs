using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public interface IResourceTextEdit : IResourceEdit
	{
		List<ITextEdit> Edits { get; }
		int ModelVersionId { get; }
		string Resource { get; }
	}
}
