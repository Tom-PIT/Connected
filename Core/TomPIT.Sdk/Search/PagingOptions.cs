﻿namespace TomPIT.Search
{
	internal class PagingOptions : ISearchPagingOptions
	{
		public int Index { get; set; } = 0;

		public int Size { get; set; } = 10;
	}
}