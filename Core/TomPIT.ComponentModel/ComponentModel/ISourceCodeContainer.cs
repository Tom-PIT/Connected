using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface ISourceCodeContainer : IElement
	{
		List<string> References();

		IText GetReference(string name);
	}
}
