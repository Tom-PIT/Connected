using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT
{
	public static class RuntimeBootstrapper
	{
		public static IApplicationBuilder UseApiExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RuntimeExceptionMiddleware>();
		}

		public static IApplicationBuilder UseAjaxExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RuntimeExceptionMiddleware>();
		}

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
