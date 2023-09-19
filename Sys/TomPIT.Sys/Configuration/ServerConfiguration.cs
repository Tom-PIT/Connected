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
			Shell.RegisterService(typeof(ISysCryptographyService), typeof(CryptographyService));
		}

		private static void InitializeAuthentication()
		{
			TomPITAuthenticationHandler.IssuerSigningKey = AuthenticationConfiguration.JwToken.IssuerSigningKey;
			TomPITAuthenticationHandler.ValidAudience = AuthenticationConfiguration.JwToken.ValidAudience;
			TomPITAuthenticationHandler.ValidIssuer = AuthenticationConfiguration.JwToken.ValidIssuer;
		}

		private static void InitializeData()
		{
			foreach (var i in StorageProviders.Items)
			{
				var t = TypeExtensions.GetType(i);

				if (t is null)
					throw new SysException($"{SR.ErrInvalidStorageProviderType} ({i})");

				var instance = t.CreateInstance<IStorageProvider>();

				if (instance is null)
					throw new SysException($"{SR.ErrInvalidStorageProviderType} ({i})", SR.ErrInvalidStorageProviderType, i);

				Shell.GetService<IStorageProviderService>().Register(instance);
			}
		}
	}
}