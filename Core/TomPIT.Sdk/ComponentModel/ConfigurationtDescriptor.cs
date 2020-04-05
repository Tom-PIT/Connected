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

		protected override void OnValidate()
		{
			//if (Configuration == null)
			//	throw new RuntimeException($"{SR.ErrCannotFindConfiguration} ({MicroServiceName}/{ComponentName})");
		}
	}
}