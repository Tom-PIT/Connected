using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Configuration;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Deployment;
using TomPIT.ComponentModel.Distributed;
using TomPIT.ComponentModel.IoC;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.Management;
using TomPIT.ComponentModel.Messaging;
using TomPIT.ComponentModel.Navigation;
using TomPIT.ComponentModel.Quality;
using TomPIT.ComponentModel.Runtime;
using TomPIT.ComponentModel.Scripting;
using TomPIT.ComponentModel.Search;
using TomPIT.ComponentModel.Security;

namespace TomPIT.Reflection;
internal class TextElementResolver
{
	public TextElementResolver(string path)
	{
		Path = path;
	}

	private string Path { get; }
	private IMicroService MicroService { get; set; }
	private string Component { get; set; }
	private string Operation { get; set; }
	private IText Result { get; set; }
	public IText Resolve()
	{
		var tokens = Path.Split('/');

		if (tokens.Length < 2)
			return null;

		MicroService = Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

		if (MicroService is null)
			return null;

		Component = TrimExtension(tokens[1]);

		if (tokens.Length > 2)
			Operation = TrimExtension(tokens[2]);

		if (string.IsNullOrEmpty(Operation))
			ResolveComponent();
		else
			ResolveElement();

		return Result;
	}

	private string TrimExtension(string value)
	{
		return System.IO.Path.GetFileNameWithoutExtension(value);
	}

	private void ResolveComponent()
	{
		if (LoadScript())
			return;

		if (LoadEntity())
			return;

		if (LoadMiddleware())
			return;

		if (LoadModel())
			return;

		if (LoadConnection())
			return;

		if (LoadHostedWorker())
			return;

		if (LoadHostedService())
			return;

		if (LoadSearchCatalog())
			return;

		if (LoadApiComponent())
			return;

		if (LoadEventComponent())
			return;

		if (LoadPermissionDescriptor())
			return;

		if (LoadSubscriptionComponent())
			return;

		if (LoadInbox())
			return;

		if (LoadSmtpConnection())
			return;

		if (LoadSettings())
			return;

		if (LoadInstaller())
			return;

		if (LoadRuntime())
			return;

		if (LoadManagement())
			return;

		if (LoadUnitTest())
			return;

		if (LoadBigDataPartition())
			return;

		if (LoadManagement())
			return;

		if (LoadIoTHub())
			return;

		if (LoadSitemap())
			return;
	}

	private void ResolveElement()
	{
		if (LoadApi())
			return;

		if (LoadEvents())
			return;

		if (LoadQueue())
			return;

		if (LoadEventBindings())
			return;

		if (LoadUIDependencyInjections())
			return;

		if (LoadIoCContainers())
			return;

		if (LoadIoCEndpoints())
			return;

		if (LoadDependencyInjections())
			return;

		if (LoadSubscriptions())
			return;
	}

	private bool LoadScript()
	{
		return LoadSimple<IScriptConfiguration>(ComponentCategories.Script);
	}

	private bool LoadEntity()
	{
		return LoadSimple<IEntityConfiguration>(ComponentCategories.Entity);
	}

	private bool LoadConnection()
	{
		return LoadSimple<IConnectionConfiguration>(ComponentCategories.Connection);
	}

	private bool LoadModel()
	{
		return LoadSimple<IModelConfiguration>(ComponentCategories.Model);
	}

	private bool LoadMiddleware()
	{
		return LoadSimple<IMiddlewareConfiguration>(ComponentCategories.Middleware);
	}

	private bool LoadApiComponent()
	{
		return LoadSimple<IApiConfiguration>(ComponentCategories.Api);
	}

	private bool LoadEventComponent()
	{
		return LoadSimple<IDistributedEventsConfiguration>(ComponentCategories.DistributedEvent);
	}

	private bool LoadHostedWorker()
	{
		return LoadSimple<IHostedWorkerConfiguration>(ComponentCategories.HostedWorker);
	}

	private bool LoadHostedService()
	{
		return LoadSimple<IHostedServiceConfiguration>(ComponentCategories.HostedService);
	}

	private bool LoadSearchCatalog()
	{
		return LoadSimple<ISearchCatalogConfiguration>(ComponentCategories.SearchCatalog);
	}
	private bool LoadPermissionDescriptor()
	{
		return LoadSimple<IPermissionDescriptorConfiguration>(ComponentCategories.PermissionDescriptor);
	}

	private bool LoadSubscriptionComponent()
	{
		return LoadSimple<ISubscriptionConfiguration>(ComponentCategories.Subscription);
	}

	private bool LoadInbox()
	{
		return LoadSimple<IInboxConfiguration>(ComponentCategories.Inbox);
	}

	private bool LoadSmtpConnection()
	{
		return LoadSimple<ISmtpConnectionConfiguration>(ComponentCategories.SmtpConnection);
	}

	private bool LoadManagement()
	{
		return LoadSimple<IManagementConfiguration>(ComponentCategories.Management);
	}

	private bool LoadInstaller()
	{
		return LoadSimple<IInstallerConfiguration>(ComponentCategories.Installer);
	}

	private bool LoadRuntime()
	{
		return LoadSimple<IRuntimeConfiguration>(ComponentCategories.Runtime);
	}

	private bool LoadSettings()
	{
		return LoadSimple<ISettingsConfiguration>(ComponentCategories.Settings);
	}

	private bool LoadUnitTest()
	{
		return LoadSimple<IUnitTestConfiguration>(ComponentCategories.UnitTest);
	}

	private bool LoadBigDataPartition()
	{
		return LoadSimple<IPartitionConfiguration>(ComponentCategories.BigDataPartition);
	}

	private bool LoadIoTHub()
	{
		return LoadSimple<IIoTHubConfiguration>(ComponentCategories.IoTHub);
	}

	private bool LoadSitemap()
	{
		return LoadSimple<ISiteMapConfiguration>(ComponentCategories.SiteMap);
	}

	private bool LoadSimple<T>(string category) where T : IText
	{
		var component = Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, category, Component);

		if (component is null)
			return false;

		Result = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IText;

		return true;
	}

	private T LoadComplex<T>(string category) where T : IConfiguration
	{
		var component = Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, category, Component);

		if (component is null)
			return default;

		return (T)Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);
	}

	private bool LoadApi()
	{
		if (LoadComplex<IApiConfiguration>(ComponentCategories.Api) is not IApiConfiguration config)
			return false;

		Result = config.Operations.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadEvents()
	{
		if (LoadComplex<IDistributedEventsConfiguration>(ComponentCategories.DistributedEvent) is not IDistributedEventsConfiguration config)
			return false;

		Result = config.Events.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadQueue()
	{
		if (LoadComplex<IQueueConfiguration>(ComponentCategories.Queue) is not IQueueConfiguration config)
			return false;

		Result = config.Workers.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadEventBindings()
	{
		if (LoadComplex<IEventBindingConfiguration>(ComponentCategories.EventBinder) is not IEventBindingConfiguration config)
			return false;

		Result = config.Events.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}
	private bool LoadUIDependencyInjections()
	{
		if (LoadComplex<IUIDependencyInjectionConfiguration>(ComponentCategories.UIDependencyInjection) is not IUIDependencyInjectionConfiguration config)
			return false;

		Result = config.Injections.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadIoCContainers()
	{
		if (LoadComplex<IIoCContainerConfiguration>(ComponentCategories.IoCContainer) is not IIoCContainerConfiguration config)
			return false;

		Result = config.Operations.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadIoCEndpoints()
	{
		if (LoadComplex<IIoCEndpointConfiguration>(ComponentCategories.IoCEndpoint) is not IIoCEndpointConfiguration config)
			return false;

		Result = config.Endpoints.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadDependencyInjections()
	{
		if (LoadComplex<IDependencyInjectionConfiguration>(ComponentCategories.DependencyInjection) is not IDependencyInjectionConfiguration config)
			return false;

		Result = config.Injections.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}

	private bool LoadSubscriptions()
	{
		if (LoadComplex<ISubscriptionConfiguration>(ComponentCategories.Subscription) is not ISubscriptionConfiguration config)
			return false;

		Result = config.Events.FirstOrDefault(f => string.Equals(f.Name, Operation, System.StringComparison.OrdinalIgnoreCase));

		return true;
	}
}
