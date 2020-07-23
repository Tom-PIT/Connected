using Microsoft.AspNetCore.Builder;
using TomPIT.Analytics;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Design;
using TomPIT.Design.Serialization;
using TomPIT.Design.Validation;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.IoC;
using TomPIT.IoT;
using TomPIT.Messaging;
using TomPIT.Navigation;
using TomPIT.Reflection;
using TomPIT.Search;
using TomPIT.Security;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.Runtime
{
	internal static class RuntimeBootstrapper
	{
		public static IApplicationBuilder UseApiExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<RuntimeExceptionMiddleware>();
		}

		public static IApplicationBuilder UseAjaxExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<AjaxExceptionMiddleware>();
		}

		public static void Run()
		{
			RegisterServices();
		}

		private static void RegisterServices()
		{
			Shell.RegisterService(typeof(IRuntimeService), typeof(RuntimeService));
			Shell.RegisterService(typeof(IConnectivityService), typeof(ConnectivityService));
			Shell.RegisterService(typeof(IMicroServiceResolutionService), typeof(MicroServiceResolutionService));

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ISerializationService), typeof(SerializationService));
			e.Tenant.RegisterService(typeof(ICompilerService), typeof(CompilerService));
			e.Tenant.RegisterService(typeof(IMicroServiceService), typeof(MicroServiceService));
			e.Tenant.RegisterService(typeof(ISettingService), typeof(SettingService));
			e.Tenant.RegisterService(typeof(INamingService), typeof(NamingService));
			e.Tenant.RegisterService(typeof(ILoggingService), typeof(LoggingService));
			e.Tenant.RegisterService(typeof(IResourceGroupService), typeof(ResourceGroupService));
			e.Tenant.RegisterService(typeof(ILanguageService), typeof(LanguageService));
			e.Tenant.RegisterService(typeof(IInstanceEndpointService), typeof(InstanceEndpointService));
			e.Tenant.RegisterService(typeof(IAuthorizationService), typeof(AuthorizationService));
			e.Tenant.RegisterService(typeof(IComponentService), typeof(ComponentService));
			e.Tenant.RegisterService(typeof(IUserService), typeof(UserService));
			e.Tenant.RegisterService(typeof(IRoleService), typeof(RoleService));
			e.Tenant.RegisterService(typeof(IStorageService), typeof(StorageService));
			e.Tenant.RegisterService(typeof(IDataProviderService), typeof(DataProviderService));
			e.Tenant.RegisterService(typeof(IEventService), typeof(EventService));
			e.Tenant.RegisterService(typeof(IAuditService), typeof(AuditService));
			e.Tenant.RegisterService(typeof(IDiscoveryService), typeof(DiscoveryService));
			e.Tenant.RegisterService(typeof(ICryptographyService), typeof(CryptographyService));
			e.Tenant.RegisterService(typeof(IMetricService), typeof(MetricService));
			e.Tenant.RegisterService(typeof(IValidationService), typeof(ValidationService));
			e.Tenant.RegisterService(typeof(IUserDataService), typeof(UserDataService));
			e.Tenant.RegisterService(typeof(IMailService), typeof(MailService));
			e.Tenant.RegisterService(typeof(ISubscriptionService), typeof(SubscriptionService));
			e.Tenant.RegisterService(typeof(IAlienService), typeof(AlienService));
			e.Tenant.RegisterService(typeof(IIoTService), typeof(IoTService));
			e.Tenant.RegisterService(typeof(IDataCachingService), typeof(DataCachingService));
			e.Tenant.RegisterService(typeof(IQueueService), typeof(QueueService));
			e.Tenant.RegisterService(typeof(IGraphicsService), typeof(GraphicsService));
			e.Tenant.RegisterService(typeof(ISearchService), typeof(SearchService));
			e.Tenant.RegisterService(typeof(ILocalizationService), typeof(LocalizationService));
			e.Tenant.RegisterService(typeof(INavigationService), typeof(NavigationService));
			e.Tenant.RegisterService(typeof(IIoCService), typeof(IoCService));
			e.Tenant.RegisterService(typeof(IPrintingService), typeof(PrintingService));
			e.Tenant.RegisterService(typeof(IUIDependencyInjectionService), typeof(UIDependencyInjectionService));
			e.Tenant.RegisterService(typeof(IDependencyInjectionService), typeof(DependencyInjectionService));
			e.Tenant.RegisterService(typeof(IAnalyticsService), typeof(AnalyticsService));
			e.Tenant.RegisterService(typeof(IModelService), typeof(ModelService));
			e.Tenant.RegisterService(typeof(IDesignService), typeof(DesignService));

			if (Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Runtime && Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
				e.Tenant.RegisterService(typeof(IMicroServiceRuntimeService), new MicroServiceRuntimeService(e.Tenant));

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
			{
				var iotClient = new IoTClient(e.Tenant, e.Tenant.AuthenticationToken);

				e.Tenant.Items.TryAdd("iotClient", iotClient);

				iotClient.Connect();

				var dataCache = new DataCachingClient(e.Tenant, e.Tenant.AuthenticationToken);

				e.Tenant.Items.TryAdd("dataCache", dataCache);

				dataCache.Connect();
			}
		}
	}
}
