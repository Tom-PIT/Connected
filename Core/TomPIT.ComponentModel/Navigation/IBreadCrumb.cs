using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Navigation
{
	public interface IBreadcrumb
	{
		string Text { get; }
		string Key { get; }
		string Url { get; }
	}
}
