using Microsoft.AspNetCore.Builder;
using TomPIT.Analysis;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Design;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.IoT;
using TomPIT.Search;
using TomPIT.Security;
using TomPIT.Services;
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

			Shell.GetService<IConnectivityService>().ConnectionRegistered += OnConnectionRegistered;
		}

		private static void OnConnectionRegistered(object sender, SysConnectionRegisteredArgs e)
		{
			e.Connection.RegisterService(typeof(ISerializationService), typeof(SerializationService));
			e.Connection.RegisterService(typeof(ICompilerService), typeof(CompilerService));
			e.Connection.RegisterService(typeof(IMicroServiceService), typeof(MicroServiceService));
			e.Connection.RegisterService(typeof(ISettingService), typeof(SettingService));
			e.Connection.RegisterService(typeof(INamingService), typeof(NamingService));
			e.Connection.RegisterService(typeof(ILoggingService), typeof(LoggingService));
			e.Connection.RegisterService(typeof(IEnvironmentUnitService), typeof(EnvironmentUnitService));
			e.Connection.RegisterService(typeof(IResourceGroupService), typeof(ResourceGroupService));
			e.Connection.RegisterService(typeof(ILanguageService), typeof(LanguageService));
			e.Connection.RegisterService(typeof(IInstanceEndpointService), typeof(InstanceEndpointService));
			e.Connection.RegisterService(typeof(IAuthorizationService), typeof(AuthorizationService));
			e.Connection.RegisterService(typeof(IComponentService), typeof(ComponentService));
			e.Connection.RegisterService(typeof(IUserService), typeof(UserService));
			e.Connection.RegisterService(typeof(IRoleService), typeof(RoleService));
			e.Connection.RegisterService(typeof(IStorageService), typeof(StorageService));
			e.Connection.RegisterService(typeof(IDataProviderService), typeof(DataProviderService));
			e.Connection.RegisterService(typeof(IEventService), typeof(EventService));
			e.Connection.RegisterService(typeof(IAuditService), typeof(AuditService));
			e.Connection.RegisterService(typeof(IDiscoveryService), typeof(DiscoveryService));
			e.Connection.RegisterService(typeof(ICryptographyService), typeof(CryptographyService));
			e.Connection.RegisterService(typeof(IMetricService), typeof(MetricService));
			e.Connection.RegisterService(typeof(IValidationService), typeof(ValidationService));
			e.Connection.RegisterService(typeof(IUserDataService), typeof(UserDataService));
			e.Connection.RegisterService(typeof(IMailService), typeof(MailService));
			e.Connection.RegisterService(typeof(ISubscriptionService), typeof(SubscriptionService));
			e.Connection.RegisterService(typeof(IAlienService), typeof(AlienService));
			e.Connection.RegisterService(typeof(IIoTService), typeof(IoTService));
			e.Connection.RegisterService(typeof(IDataCachingService), typeof(DataCachingService));
			e.Connection.RegisterService(typeof(IQueueService), typeof(QueueService));
			e.Connection.RegisterService(typeof(IGraphicsService), typeof(GraphicsService));
			e.Connection.RegisterService(typeof(ISearchService), typeof(SearchService));
			e.Connection.RegisterService(typeof(ILocalizationService), typeof(LocalizationService));

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
			{
				var iotClient = new IoTClient(e.Connection, e.Connection.AuthenticationToken);

				e.Connection.Items.TryAdd("iotClient", iotClient);

				iotClient.Connect();

				var dataCache = new DataCachingClient(e.Connection, e.Connection.AuthenticationToken);

				e.Connection.Items.TryAdd("dataCache", dataCache);

				dataCache.Connect();
			}
		}
	}
}
