using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Navigation
{
	public interface ISiteMapElement
	{
		ISiteMapElement Parent { get; }
		string Text { get; }
	}
}
