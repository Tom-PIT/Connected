using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.ComponentModel
{
	public class ConfigurationDescriptor<T> : ComponentDescriptor
	{
		private T _configuration = default;

		public ConfigurationDescriptor(string identifier, string componentCategory) : this(null, identifier, componentCategory)
		{
		}

		public ConfigurationDescriptor(IMiddlewareContext context, string identifier, string componentCategory) : base(context, identifier, componentCategory)
		{
		}

		public T Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = (T)Context.Tenant.GetService<IComponentService>().SelectConfiguration(MicroService.Token, Category, ComponentName);

				return _configuration;
			}
		}

		public void ValidateConfiguration()
		{
			if (Configuration == null)
				throw new NotFoundException($"{SR.ErrCannotFindConfiguration} ({MicroServiceName}/{ComponentName})");
		}
	}
}