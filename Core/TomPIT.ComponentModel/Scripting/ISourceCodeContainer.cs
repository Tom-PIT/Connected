using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.Scripting
{
	[Obsolete]
	public interface ISourceCodeContainer : IElement
	{
		List<string> References();

		IText GetReference(string name);
	}
}
