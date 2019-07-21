using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public enum CatalogStateStatus
	{
		Pending = 1,
		Rebuilding = 2
	}
	public interface ICatalogState
	{
		Guid Catalog { get; }
		CatalogStateStatus Status { get; }
	}
}
