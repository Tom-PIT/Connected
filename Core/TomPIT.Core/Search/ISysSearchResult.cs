using System;

namespace TomPIT.Search
{
	public interface ISysSearchResult
	{
		Guid MicroService { get; }
		Guid Component { get; }
		string ComponentName { get; }
		Guid Element { get; }
		string ElementName { get; }
		string Title { get; }
		string Content { get; }
		string FormattedContent { get; }
		string Tags { get; }
	}
}
