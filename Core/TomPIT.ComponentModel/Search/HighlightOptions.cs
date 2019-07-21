using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	internal class HighlightOptions : ISearchHighlightOptions
	{
		public bool Enabled { get; set; } = true;
		public string PreTag { get; set; } = "<span style=\"font-weight:bold;\">";
		public string PostTag { get; set; } = "</span>";
	}
}
