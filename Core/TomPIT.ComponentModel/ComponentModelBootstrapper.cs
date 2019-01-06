using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.DataProviders;
using TomPIT.Configuration;
using TomPIT.Design;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Globalization;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT
{
	public static class ComponentModelBootstrapper
	{
		public static void Run()
		{
			RegisterServices();
		}

		private static void RegisterServices()
		{
			Shell.RegisterService(typeof(IRuntimeService), typeof(RuntimeService));
			Shell.RegisterService(typeof(IConnectivityService), typeof(ConnectivityService));

			Shell.GetService<IConnectivityService>().ContextRegistered += OnContextRegistered;
		}

		private static void OnContextRegistered(object sender, SysContextRegisteredArgs e)
		{
			e.Context.RegisterService(typeof(ISerializationService), typeof(SerializationService));
			e.Context.RegisterService(typeof(ICompilerService), typeof(CompilerService));
			e.Context.RegisterService(typeof(IMicroServiceService), typeof(MicroServiceService));
			e.Context.RegisterService(typeof(IFeatureService), typeof(FeatureService));
			e.Context.RegisterService(typeof(ISettingService), typeof(SettingService));
			e.Context.RegisterService(typeof(INamingService), typeof(NamingService));
			e.Context.RegisterService(typeof(ILoggingService), typeof(LoggingService));
			e.Context.RegisterService(typeof(IEnvironmentUnitService), typeof(EnvironmentUnitService));
			e.Context.RegisterService(typeof(IResourceGroupService), typeof(ResourceGroupService));
			e.Context.RegisterService(typeof(ILanguageService), typeof(LanguageService));
			e.Context.RegisterService(typeof(IInstanceEndpointService), typeof(InstanceEndpointService));
			e.Context.RegisterService(typeof(IAuthorizationService), typeof(AuthorizationService));
			e.Context.RegisterService(typeof(IComponentService), typeof(ComponentService));
			e.Context.RegisterService(typeof(IUserService), typeof(UserService));
			e.Context.RegisterService(typeof(IRoleService), typeof(RoleService));
			e.Context.RegisterService(typeof(IStorageService), typeof(StorageService));
			e.Context.RegisterService(typeof(IDataProviderService), typeof(DataProviderService));
			e.Context.RegisterService(typeof(IEventService), typeof(EventService));
			e.Context.RegisterService(typeof(IAuditService), typeof(AuditService));
			e.Context.RegisterService(typeof(IDiscoveryService), typeof(DiscoveryService));
		}
	}
}
