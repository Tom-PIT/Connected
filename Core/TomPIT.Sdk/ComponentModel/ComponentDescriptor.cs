using System;
using System.Linq;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Distributed;
using TomPIT.ComponentModel.IoC;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.Resources;
using TomPIT.ComponentModel.Scripting;
using TomPIT.ComponentModel.Search;
using TomPIT.ComponentModel.UI;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.ComponentModel
{
	public class ComponentDescriptor
	{
		private IMicroService _microService = null;
		private IComponent _component = null;

		public ComponentDescriptor(string identifier, string componentCategory) : this(null, identifier, componentCategory)
		{

		}
		public ComponentDescriptor(IMiddlewareContext context, string identifier, string componentCategory)
		{
			Context = context as IMicroServiceContext;
			Category = componentCategory;

			if (string.IsNullOrWhiteSpace(identifier))
				return;

			var tokens = identifier.Split('/');

			if (tokens.Length == 1)
			{
				if (context == null)
					throw new RuntimeException($"{SR.ErrInvalidQualifier}, {SR.ErrInvalidQualifierExpected}: 'microService/component'");

				ComponentName = tokens[0];
				MicroServiceName = Context.MicroService.Name;
				_microService = Context.MicroService;
			}
			else if (tokens.Length > 1)
			{
				MicroServiceName = tokens[0];
				ComponentName = tokens[1];

				if (tokens.Length > 2)
					Element = string.Join('/', tokens.Skip(2));
			}

			if (Context == null)
			{
				var tenant = MiddlewareDescriptor.Current.Tenant;

				_microService = tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

				if (_microService == null)
				{
					if (Guid.TryParse(MicroServiceName, out Guid ms))
					{
						_microService = tenant.GetService<IMicroServiceService>().Select(ms);

						if (_microService != null)
							MicroServiceName = _microService.Name;
					}

					if (_microService == null)
						throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({MicroServiceName})");
				}

				Context = new MicroServiceContext(_microService, tenant.Url);
			}
		}

		public string Element { get; private set; }
		public string Category { get; }
		protected IMicroServiceContext Context { get; }
		public string ComponentName { get; private set; }
		public string MicroServiceName { get; private set; }

		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
				{
					_microService = Context.Tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

					if (_microService == null && Guid.TryParse(MicroServiceName, out Guid mn))
					{
						_microService = Context.Tenant.GetService<IMicroServiceService>().Select(mn);

						if (_microService != null)
							MicroServiceName = _microService.Name;
					}
				}

				return _microService;
			}
		}

		public IComponent Component
		{
			get
			{
				if (_component == null)
				{
					_component = Context.Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, Category, ComponentName);

					if (_component == null && Guid.TryParse(ComponentName, out Guid cn))
					{
						_component = Context.Tenant.GetService<IComponentService>().SelectComponent(cn);

						if (_component != null)
							ComponentName = _component.Name;
					}
				}

				return _component;
			}
		}

		public void Validate()
		{
			if (MicroService == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({MicroServiceName})");

			if (Context != null)
				Context.MicroService.ValidateMicroServiceReference(MicroServiceName);

			if (Component == null)
				throw new RuntimeException($"{SR.ErrComponentNotFound} ({ComponentName})");

			OnValidate();
		}

		protected virtual void OnValidate()
		{

		}

		public static ConfigurationDescriptor<IViewConfiguration> View(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IViewConfiguration>(context, identifier, ComponentCategories.View);
		}

		public static ConfigurationDescriptor<ISubscriptionConfiguration> Subscription(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<ISubscriptionConfiguration>(context, identifier, ComponentCategories.Subscription);
		}

		public static ConfigurationDescriptor<IQueueConfiguration> Queue(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IQueueConfiguration>(context, identifier, ComponentCategories.Queue);
		}

		public static ConfigurationDescriptor<IDistributedEventsConfiguration> DistributedEvent(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IDistributedEventsConfiguration>(context, identifier, ComponentCategories.DistributedEvent);
		}

		public static ConfigurationDescriptor<IScriptConfiguration> Script(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IScriptConfiguration>(context, identifier, ComponentCategories.Script);
		}

		public static ConfigurationDescriptor<IPartitionConfiguration> BigDataPartition(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IPartitionConfiguration>(context, identifier, ComponentCategories.Script);
		}

		public static ConfigurationDescriptor<IApiConfiguration> Api(IMiddlewareContext context, string identifier)
		{
			var qualifiers = identifier.Count(f => f == '/');

			if (qualifiers < 2)
			{
				if (!(context is IMicroServiceContext msc))
					throw new RuntimeException(nameof(ComponentDescriptor), $"{SR.ErrApiDoubleQualifier} ({identifier})");

				if (msc.MicroService == null)
					throw new RuntimeException(nameof(ComponentDescriptor), SR.ErrMicroServiceExpected);

				identifier = identifier.Insert(0, $"{msc.MicroService.Name}/");
			}

			return new ConfigurationDescriptor<IApiConfiguration>(context, identifier, ComponentCategories.Api);
		}

		public static ConfigurationDescriptor<IDataHubConfiguration> DataHub(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IDataHubConfiguration>(context, identifier, ComponentCategories.DataHub);
		}

		public static ConfigurationDescriptor<IConnectionConfiguration> Connection(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IConnectionConfiguration>(context, identifier, ComponentCategories.Connection);
		}

		public static ConfigurationDescriptor<IIoTHubConfiguration> IoTHub(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IIoTHubConfiguration>(context, identifier, ComponentCategories.IoTHub);
		}

		public static ConfigurationDescriptor<ISearchCatalogConfiguration> SearchCatalog(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<ISearchCatalogConfiguration>(context, identifier, ComponentCategories.SearchCatalog);
		}

		public static ConfigurationDescriptor<IStringTableConfiguration> StringTable(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IStringTableConfiguration>(context, identifier, ComponentCategories.StringTable);
		}

		public static ConfigurationDescriptor<IMailTemplateConfiguration> MailTemplate(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IMailTemplateConfiguration>(context, identifier, ComponentCategories.MailTemplate);
		}

		public static ConfigurationDescriptor<IMediaResourcesConfiguration> Media(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IMediaResourcesConfiguration>(context, identifier, ComponentCategories.Media);
		}

		public static ConfigurationDescriptor<IPartialViewConfiguration> Partial(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IPartialViewConfiguration>(context, identifier, ComponentCategories.Partial);
		}

		public static ConfigurationDescriptor<IIoCContainerConfiguration> IoCContainer(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IIoCContainerConfiguration>(context, identifier, ComponentCategories.IoCContainer);
		}
	}
}
