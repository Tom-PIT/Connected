using TomPIT.Api.ComponentModel;
using TomPIT.Api.Storage;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Data;
using TomPIT.Sys.Security;
using TomPIT.Sys.Services;

namespace TomPIT.Sys.Configuration
{
	internal static class ServerConfiguration
	{
		public static void Initialize()
		{
			RegisterServices();
			InitializeAuthentication();
			InitializeData();
		}

		private static void RegisterServices()
		{
			//var providers = new List<Runtime.IServiceProvider>();

			//foreach (var i in Api.Configuration.Root.GetSection("serviceProviders").GetChildren())
			//	providers.Add(Types.GetType(i.Value).CreateInstance<Runtime.IServiceProvider>());

			Shell.RegisterService(typeof(IDatabaseService), typeof(DatabaseService));
			Shell.RegisterService(typeof(IStorageProviderService), typeof(StorageProviderService));
			Shell.RegisterService(typeof(INamingService), typeof(NamingService));
			Shell.RegisterService(typeof(ICryptographyService), typeof(CryptographyService));
		}

		private static void InitializeAuthentication()
		{
			var section = Api.Configuration.Root.GetSection("authentication");
			var jwt = section.GetSection("jwToken");

			TomPITAuthenticationHandler.IssuerSigningKey = jwt.GetSection("issuerSigningKey").Value;
			TomPITAuthenticationHandler.ValidAudience = jwt.GetSection("validAudience").Value;
			TomPITAuthenticationHandler.ValidIssuer = jwt.GetSection("validIssuer").Value;
		}

		private static void InitializeData()
		{
			foreach (var i in Api.Configuration.Root.GetSection("storageProviders").GetChildren())
			{
				var t = Types.GetType(i.Value);

				if (t == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrInvalidStorageProviderType, i.Value));

				var instance = t.CreateInstance<IStorageProvider>();

				if (instance == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrInvalidStorageProviderType, i.Value));

				Shell.GetService<IStorageProviderService>().Register(instance);
			}
		}
	}
}