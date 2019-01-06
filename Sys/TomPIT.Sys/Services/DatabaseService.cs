using System;
using TomPIT.Data.Sql;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb;
using TomPIT.SysDb.Sql;

namespace TomPIT.Sys.Services
{
	internal class DatabaseService : IDatabaseService
	{
		public DatabaseService()
		{
			Configure();
		}

		private void Configure()
		{
			var setting = TomPIT.Sys.Api.Configuration.Root["database"];

			if (!string.IsNullOrWhiteSpace(setting))
			{
				var type = Type.GetType(setting);

				if (type != null)
					Proxy = type.Assembly.CreateInstance(type.FullName) as ISysDbProxy;
			}

			if (Proxy == null)
				Proxy = new SqlProxy();

			if (Proxy is SqlProxy)
				ConnectionBase.DefaultConnectionString = TomPIT.Sys.Api.Configuration.Root["connectionStrings:sys"];
		}

		public ISysDbProxy Proxy
		{
			get; set;
		}
	}
}