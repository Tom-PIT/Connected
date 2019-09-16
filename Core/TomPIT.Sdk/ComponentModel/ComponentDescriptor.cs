using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Distributed;
using TomPIT.ComponentModel.Scripting;
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
			Context = context;
			Category = componentCategory;

			if (string.IsNullOrWhiteSpace(identifier))
				return;

			var tokens = identifier.Split('/');

			if (tokens.Length == 1)
			{
				if (context == null)
					throw new RuntimeException($"{SR.ErrInvalidQualifier}, {SR.ErrInvalidQualifierExpected}: 'microService/component'");

				ComponentName = tokens[0];
				MicroServiceName = context.MicroService.Name;
				_microService = context.MicroService;
			}
			else if (tokens.Length > 1)
			{
				MicroServiceName = tokens[0];
				ComponentName = tokens[1];

				if (tokens.Length > 2)
					Element = tokens[2];
			}

			if (Context == null)
			{
				var tenant = MiddlewareDescriptor.Current.Tenant;

				_microService = tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

				if (_microService == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({MicroServiceName})");

				Context = new MiddlewareContext(tenant.Url, _microService);
			}
		}

		public string Element { get; private set; }
		public string Category { get; }
		protected IMiddlewareContext Context { get; }
		public string ComponentName { get; }
		public string MicroServiceName { get; }

		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Context.Tenant.GetService<IMicroServiceService>().Select(MicroServiceName);

				return _microService;
			}
		}

		public IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Context.Tenant.GetService<IComponentService>().SelectComponent(MicroService.Token, Category, ComponentName);

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

		public static ConfigurationDescriptor<IDistributedEventConfiguration> Event(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IDistributedEventConfiguration>(context, identifier, ComponentCategories.Event);
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
			return new ConfigurationDescriptor<IApiConfiguration>(context, identifier, ComponentCategories.Api);
		}

		public static ConfigurationDescriptor<IConnectionConfiguration> Connection(IMiddlewareContext context, string identifier)
		{
			return new ConfigurationDescriptor<IConnectionConfiguration>(context, identifier, ComponentCategories.Connection);
		}
	}
}
