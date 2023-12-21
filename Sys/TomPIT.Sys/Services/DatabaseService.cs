using Microsoft.Extensions.Configuration;

using System.Configuration;
using System.Text.Json;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Configuration;
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
			var database = Shell.Configuration.GetValue<string>("database");

			if (string.IsNullOrWhiteSpace(database))
				throw new ConfigurationErrorsException("'database' configuration element does not have a value.");

			var type = Reflection.TypeExtensions.GetType(database);

			if (type is null)
				throw new ConfigurationErrorsException($"{SR.ErrTypeNull} (Sys Database '{database}')");

			Proxy = type.Assembly.CreateInstance(type.FullName) as ISysDbProxy;

			Proxy.Initialize(ConnectionStringsConfiguration.Sys);
		}

		public ISysDbProxy Proxy { get; set; }
	}
}