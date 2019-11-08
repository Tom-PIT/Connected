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
			var sys = Shell.GetConfiguration<IServerSys>();

			if (!string.IsNullOrWhiteSpace(sys.Database))
			{
				var type = Reflection.TypeExtensions.GetType(sys.Database);

				if (type != null)
					Proxy = type.Assembly.CreateInstance(type.FullName) as ISysDbProxy;
			}

			Proxy.Initialize(sys.ConnectionStrings.Sys);
		}

		public ISysDbProxy Proxy
		{
			get; set;
		}
	}
}