using TomPIT.Api.ComponentModel;
using TomPIT.Api.Storage;
using TomPIT.Reflection;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
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
			Shell.RegisterService(typeof(IDatabaseService), typeof(DatabaseService));
			Shell.RegisterService(typeof(IStorageProviderService), typeof(StorageProviderService));
			Shell.RegisterService(typeof(INamingService), typeof(NamingService));
			Shell.RegisterService(typeof(ICryptographyService), typeof(CryptographyService));
		}

		private static void InitializeAuthentication()
		{
			var sys = Shell.GetConfiguration<IServerSys>();

			TomPITAuthenticationHandler.IssuerSigningKey = sys.Authentication.JwToken.IssuerSigningKey;
			TomPITAuthenticationHandler.ValidAudience = sys.Authentication.JwToken.ValidAudience;
			TomPITAuthenticationHandler.ValidIssuer = sys.Authentication.JwToken.ValidIssuer;
		}

		private static void InitializeData()
		{
			var sys = Shell.GetConfiguration<IServerSys>();

			foreach (var i in sys.StorageProviders)
			{
				var t = TypeExtensions.GetType(i);

				if (t == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrInvalidStorageProviderType, i));

				var instance = t.CreateInstance<IStorageProvider>();

				if (instance == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrInvalidStorageProviderType, i));

				Shell.GetService<IStorageProviderService>().Register(instance);
			}
		}
	}
}