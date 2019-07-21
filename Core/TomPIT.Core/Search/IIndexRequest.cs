using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface IIndexRequest
	{
		Guid Identifier { get; }
		string Catalog { get; }
		DateTime Created { get; }
		string Arguments { get; }
		Guid MicroService { get; }
	}
}
