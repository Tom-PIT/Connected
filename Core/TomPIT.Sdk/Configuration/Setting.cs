using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Configuration
{
	public class Setting<T> : MiddlewareComponent, ISetting<T>
	{
		protected Setting(ISettingsMiddleware owner, string name)
		{
			Name = name;
			Owner = owner;

			ReflectionExtensions.SetPropertyValue(this, nameof(Context), owner.Context);
		}

		public string Name { get; private set; }
		protected ISettingsMiddleware Owner { get; }
		public T Value { get; set; }
	}
}
