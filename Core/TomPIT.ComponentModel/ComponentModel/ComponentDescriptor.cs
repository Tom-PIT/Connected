using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class ComponentDescriptor
	{
		private IMicroService _microService = null;
		private IComponent _component = null;

		public ComponentDescriptor(IDataModelContext context, string identifier, string componentCategory)
		{
			Context = context;
			Category = componentCategory;

			if (string.IsNullOrWhiteSpace(identifier))
				return;

			var tokens = identifier.Split('/');

			if (tokens.Length == 1)
			{
				ComponentName = tokens[0];
				MicroServiceName = context.MicroService.Name;
				_microService = context.MicroService;
			}
			else if (tokens.Length == 2)
			{
				MicroServiceName = tokens[0];
				ComponentName = tokens[1];
			}
		}

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

		public static ConfigurationDescriptor<IView> View(IDataModelContext context, string identifier)
		{
			return new ConfigurationDescriptor<IView>(context, identifier, ComponentCategories.View);
		}
	}
}
