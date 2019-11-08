using System;
using TomPIT.Search;

namespace TomPIT.Sys.Search
{
	internal class SysSearchResult : ISysSearchResult
	{
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public string ComponentName { get; set; }
		public Guid Element { get; set; }
		public string ElementName { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public string FormattedContent { get; set; }
		public string Tags { get; set; }
		internal float Score { get; set; }
	}
}