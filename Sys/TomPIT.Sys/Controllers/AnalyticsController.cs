using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Analytics;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class AnalyticsController : SysController
	{
		[HttpPost]
		public void ModifyMru()
		{
			var body = FromBody();
			var type = body.Required<int>("type");
			var primaryKey = body.Required<string>("primaryKey");
			var entity = body.Required<AnalyticsEntity>("entity");
			var entityPrimaryKey = body.Required<string>("entityPrimaryKey");
			var tags = body.Required<List<string>>("tags");
			var capacity = body.Optional("capacity", 10);

			DataModel.Mrus.Modify(type, primaryKey, entity, entityPrimaryKey, tags, capacity);
		}

		[HttpPost]
		public List<IMru> QueryMru()
		{
			var body = FromBody();
			var entity = body.Required<AnalyticsEntity>("entity");
			var entityPrimaryKey = body.Required<string>("entityPrimaryKey");
			var tags = body.Required<List<string>>("tags");

			return DataModel.Mrus.Query(entity, entityPrimaryKey, tags);
		}
	}
}
