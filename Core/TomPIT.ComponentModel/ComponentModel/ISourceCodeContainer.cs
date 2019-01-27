using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface ISourceCodeContainer
	{
		List<string> References(IPartialSourceCode sender);

		IText GetReference(string name);
	}
}
