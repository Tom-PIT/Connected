using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchHighlightOptions
	{
		bool Enabled { get; set; }
		string PreTag { get; set; }
		string PostTag { get; set; }
	}
}
