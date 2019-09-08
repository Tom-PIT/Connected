using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Handlers;
using TomPIT.ComponentModel.UI;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class ComponentDescriptor
	{
		private IMicroService _microService = null;
		private IComponent _component = null;

		public ComponentDescriptor(string identifier, string componentCategory) : this(null, identifier, componentCategory)
		{

		}
		public ComponentDescriptor(IDataModelContext context, string identifier, string componentCategory)
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

			if(Context==null)
			{
				_microService = SysExtensions.CurrentConnection().GetService<IMicroServiceService>().Select(MicroServiceName);

				if (_microService == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({MicroServiceName})");

				Context = new DataModelContext(ExecutionContext.Create(SysExtensions.CurrentConnection().Url, _microService));
			}
		}

		public string Element { get; private set; }
		public string Category { get; }
		protected IDataModelContext Context { get; }
		public string ComponentName { get; }
		public string MicroServiceName { get; }

		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Context.Connection().GetService<IMicroServiceService>().Select(MicroServiceName);

				return _microService;
			}
		}

		public IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Context.Connection().GetService<IComponentService>().SelectComponent(MicroService.Token, Category, ComponentName);

				return _component;
			}
		}

		public void Validate()
		{
			if (MicroService == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({MicroServiceName})");

			if (Context != null)
				Context.MicroService.ValidateMicroServiceReference(Context.Connection(), MicroServiceName);

			if (Component == null)
				throw new RuntimeException($"{SR.ErrComponentNotFound} ({ComponentName})");

			OnValidate();
		}

		protected virtual void OnValidate()
		{

		}

		public static ConfigurationDescriptor<IView> View(IDataModelContext context, string identifier)
		{
			return new ConfigurationDescriptor<IView>(context, identifier, ComponentCategories.View);
		}

		public static ConfigurationDescriptor<ISubscription> Subscription(IDataModelContext context, string identifier)
		{
			return new ConfigurationDescriptor<ISubscription>(context, identifier, ComponentCategories.Subscription);
		}

		public static ConfigurationDescriptor<IQueueHandlerConfiguration> Queue(IDataModelContext context, string identifier)
		{
			return new ConfigurationDescriptor<IQueueHandlerConfiguration>(context, identifier, ComponentCategories.Queue);
		}

		public static ConfigurationDescriptor<IDistributedEvent> Event(IDataModelContext context, string identifier)
		{
			return new ConfigurationDescriptor<IDistributedEvent>(context, identifier, ComponentCategories.Event);
		}
	}
}
