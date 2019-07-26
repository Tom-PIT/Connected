using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchParserOptions
	{
		bool AllowLeadingWildcard { get; }
	}
}
