using System;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb;

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
			var setting = Api.Configuration.Root["database"];

			if (!string.IsNullOrWhiteSpace(setting))
			{
				var type = Type.GetType(setting);

				if (type != null)
					Proxy = type.Assembly.CreateInstance(type.FullName) as ISysDbProxy;
			}

			Proxy.Initialize(Api.Configuration.Root["connectionStrings:sys"]);
		}

		public ISysDbProxy Proxy
		{
			get; set;
		}
	}
}