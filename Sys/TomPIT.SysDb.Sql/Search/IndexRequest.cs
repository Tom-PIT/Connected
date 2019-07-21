using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Data.Sql;
using TomPIT.Search;
using TomPIT.SysDb.Search;

namespace TomPIT.SysDb.Sql.Search
{
	internal class IndexRequest : LongPrimaryKeyRecord, IIndexRequest
	{
		public Guid Identifier {get;set;}

		public string Catalog {get;set;}

		public DateTime Created {get;set;}

		public string Arguments {get;set;}

		public Guid MicroService {get;set;}

		protected override void OnCreate()
		{
			base.OnCreate();

			Identifier = GetGuid("identifier");

			if (IsDefined("arguments"))
				Arguments = GetString("arguments");

			Catalog = GetString("catalog");
			Created = GetDate("created");
			MicroService = GetGuid("service");
		}
	}
}
