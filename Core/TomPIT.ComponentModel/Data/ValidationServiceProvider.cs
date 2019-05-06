using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Data
{
	internal class ValidationServiceProvider : IServiceProvider
	{
		private Dictionary<Type, object> _services = null;

		public object GetService(Type serviceType)
		{
			return Services[serviceType];
		}

		private Dictionary<Type, object> Services
		{
			get
			{
				if (_services == null)
					_services = new Dictionary<Type, object>();

				return _services;
			}
		}

		public void AddService(Type type, object instance)
		{
			Services[type] = instance;
		}
	}
}
