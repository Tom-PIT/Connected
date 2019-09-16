using System.Collections.Generic;

namespace TomPIT.ComponentModel.Scripting
{
	public interface ISourceCodeContainer : IElement
	{
		List<string> References();

		IText GetReference(string name);
	}
}
