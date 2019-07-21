using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchResultDescriptor
	{
		string Title { get; }
		int Lcid { get; }
		string Tags { get; }
		Guid User { get; }
		DateTime Date { get; }
		Guid Catalog { get; }
		string Text { get; }
		string Id { get; }
		string Content { get; }
	}
}