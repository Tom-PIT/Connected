using System.Collections.Generic;
using System.Linq;
using TomPIT.IoC;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Interop
{
	internal static class DependencyInjectionExtensions
	{
		public static T Invoke<T>(this List<IDependencyInjectionObject> items, T e)
		{
			var result = e;

			foreach (var dependency in items)
			{
				if (dependency is IDependencyInjectionMiddleware im)
					im.Invoke(result);
				else
				{
					if (!dependency.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDependencyInjectionMiddleware<>)))
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

		public static void Commit(this List<IDependencyInjectionObject> items)
		{
			foreach (var dependency in items)
				dependency.Commit();
		}

		public static void Validate(this List<IDependencyInjectionObject> items)
		{
			foreach (var dependency in items)
				dependency.Validate();
		}

		public static void Authorize(this List<IDependencyInjectionObject> items)
		{
			foreach (var dependency in items)
				dependency.Authorize();
		}

		public static T Authorize<T>(this List<IDependencyInjectionObject> items, T e)
		{
			var result = e;

			foreach (var dependency in items)
			{
				if (!dependency.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDependencyInjectionMiddleware<>)))
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
