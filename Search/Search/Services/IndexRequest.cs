using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Search.Services
{
	internal class IndexRequest : IIndexRequest
	{
		public Guid Identifier {get;set;}
		public string Catalog {get;set;}
		public DateTime Created {get;set;}
		public string Arguments {get;set;}
		public Guid MicroService {get;set;}
	}
}
