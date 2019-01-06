using System;
using TomPIT.Api.Net;
using TomPIT.Data.Sql;

namespace TomPIT.StorageProvider.Sql
{
	internal class Topic : LongPrimaryKeyRecord, ITopic
	{
		public string Name { get; set; }
		public Guid ResourceGroup { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			ResourceGroup = GetGuid("resource_group_token");
		}
	}
}