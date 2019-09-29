using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface IResourceTextEdit : IResourceEdit
	{
		List<ITextEdit> Edits { get; }
		int ModelVersionId { get; }
		string Resource { get; }
	}
}
