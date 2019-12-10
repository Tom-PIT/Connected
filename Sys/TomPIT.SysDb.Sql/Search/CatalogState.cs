using System;
using TomPIT.Data.Sql;
using TomPIT.Search;

namespace TomPIT.SysDb.Sql.Search
{
	internal class CatalogState : PrimaryKeyRecord, ICatalogState
	{
		public Guid Catalog { get; set; }
		public CatalogStateStatus Status { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Catalog = GetGuid("catalog");
			Status = GetValue("status", CatalogStateStatus.Pending);
		}
	}
}
