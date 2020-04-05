using System.Collections.Generic;
using System.Linq;
using TomPIT.IoC;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Interop
{
	internal static class DependencyInjectionExtensions
	{
		public static T Invoke<T>(this List<IApiDependencyInjectionObject> items, T e)
		{
			var result = e;

			foreach (var dependency in items)
			{
				if (dependency is IApiDependencyInjectionMiddleware im)
					im.Invoke(result);
				else
				{
					if (!dependency.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IApiDependencyInjectionMiddleware<>)))
						continue;

					var methods = dependency.GetType().GetMethods().Where(f => string.Compare(f.Name, "Invoke", false) == 0);

					foreach (var m in methods)
					{
						if (!m.ContainsGenericParameters || m.GetParameters().Length != 1)
							continue;

						result = Marshall.Convert<T>(m.Invoke(dependency, new object[] { result }));
					}
				}
			}

			return result;
		}

		public static T Authorize<T>(this List<IApiDependencyInjectionObject> items, T e)
		{
			var result = e;

			foreach (var dependency in items)
			{
				if (!dependency.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IApiDependencyInjectionMiddleware<>)))
					continue;

				var methods = dependency.GetType().GetMethods().Where(f => string.Compare(f.Name, "Authorize", false) == 0);

				foreach (var m in methods)
				{
					if (!m.ContainsGenericParameters || m.GetParameters().Length != 1)
						continue;

					result = Marshall.Convert<T>(m.Invoke(dependency, new object[] { result }));
				}
			}

			return result;
		}
	}
}
