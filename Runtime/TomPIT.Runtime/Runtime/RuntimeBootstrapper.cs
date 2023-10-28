using Microsoft.AspNetCore.Builder;
using TomPIT.Analytics;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Cdn.Documents;
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
using TomPIT.Distributed;
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
			Shell.RegisterService(typeof(IRuntimeService), typeof(RuntimeService));
			Shell.RegisterService(typeof(IConnectivityService), typeof(ConnectivityService));
			Shell.RegisterService(typeof(IMicroServiceResolutionService), typeof(MicroServiceResolutionService));

			Shell.GetService<IConnectivityService>().TenantInitialize += OnTenantInitialize;
			Shell.GetService<IConnectivityService>().TenantInitialized += OnTenantInitialized;
		}

		private static void OnTenantInitialized(object sender, TenantArgs e)
		{
			foreach (var i in Tenant.GetService<IDesignService>().QueryDesigners())
			{
				var t = TypeExtensions.GetType(i);

				if (t == null)
					continue;

				var template = t.CreateInstance<IMicroServiceTemplate>();

				if (template != null)
					e.Tenant.GetService<IMicroServiceTemplateService>().Register(template);
			}
		}

		private static void OnTenantInitialize(object sender, TenantArgs e)
		{
			e.Tenant.RegisterService(typeof(ISerializationService), typeof(SerializationService));
			e.Tenant.RegisterService(typeof(ICompilerService), typeof(CompilerService));
			e.Tenant.RegisterService(typeof(INuGetService), typeof(NuGetService));
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
			e.Tenant.RegisterService(typeof(ICdnService), typeof(CdnService));
			e.Tenant.RegisterService(typeof(ILockingService), typeof(LockingService));
			e.Tenant.RegisterService(typeof(IClientService), typeof(ClientService));
			e.Tenant.RegisterService(typeof(IDocumentService), typeof(DocumentService));
			e.Tenant.RegisterService(typeof(IFileSystemService), typeof(FileSystemService));
			e.Tenant.RegisterService(typeof(IMicroServiceTemplateService), typeof(MicroServiceTemplateService));
			e.Tenant.RegisterService(typeof(IWorkerService), typeof(WorkerService));
			e.Tenant.RegisterService(typeof(IMicroServiceRuntimeService), new MicroServiceRuntimeService(e.Tenant));
			e.Tenant.RegisterService(typeof(IDebugService), typeof(DebugService));

			MicroServiceCompiler.Compile();

			if (!string.IsNullOrEmpty(e.Tenant.Url))
			{
				if (Instance.Features.HasFlag(InstanceFeatures.IoT))
				{
					var iotClient = new IoTClient(e.Tenant, e.Tenant.AuthenticationToken);

					e.Tenant.Items.TryAdd("iotClient", iotClient);

					iotClient.Connect();
				}

				var dataCache = new DataCachingClient(e.Tenant, e.Tenant.AuthenticationToken);

				e.Tenant.Items.TryAdd("dataCache", dataCache);

				dataCache.Connect();
			}
		}
	}
}
