using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search
{
	internal class CatalogState : ICatalogState
	{
		public Guid Catalog {get;set;}
		public CatalogStateStatus Status {get;set;}
	}
}
