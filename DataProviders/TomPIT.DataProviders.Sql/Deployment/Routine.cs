﻿using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class Routine : SchemaBase, IRoutine
	{
		public string Type { get; set; }
		public string Definition { get; set; }
	}
}
