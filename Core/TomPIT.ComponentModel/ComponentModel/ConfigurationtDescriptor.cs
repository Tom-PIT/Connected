using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class ConfigurationDescriptor<T> : ComponentDescriptor
	{
		private T _configuration = default;

		public ConfigurationDescriptor(IDataModelContext context, string identifier, string componentCategory) : base(context, identifier, componentCategory)
		{
		}

		public T Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = (T)Context.Connection().GetService<IComponentService>().SelectConfiguration(MicroService.Token, Category, ComponentName);

				return _configuration;
			}
		}
	}
}