using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class ResultsOptions : ISearchResultsOptions
	{
		public int TextMaxLength { get; set; } = 255;
	}
}
