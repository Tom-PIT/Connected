using System.Linq;
using System.Reflection;
using TomPIT.Dynamic;
using TomPIT.Exceptions;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareIoC : IoCObject, IMiddlewareIoC
	{
		public MiddlewareIoC()
		{
		}

		public MiddlewareIoC(object instance) : base(instance)
		{
		}

		T IMiddlewareIoC.GetValue<T>(string propertyName)
		{
			var property = ResolveProperty(this, propertyName.Split('.'));

			return (T)property.Item1.GetValue(property.Item2);
		}

		void IMiddlewareIoC.Invoke(string methodName, params object[] args)
		{
			var method = ResolveMethod(methodName);

			method.Item1.Invoke(method.Item2, args);
		}

		T IMiddlewareIoC.Invoke<T>(string methodName, params object[] args)
		{
			var method = ResolveMethod(methodName);

			var r = method.Item1.Invoke(method.Item2, args);

			return Types.Convert<T>(r);
		}

		void IMiddlewareIoC.SetValue<T>(string propertyName, T value)
		{
			var property = ResolveProperty(this, propertyName.Split('.'));

			if (property.Item1 == null)
				return;

			property.Item1.SetValue(property.Item2, value);
		}

		private (MethodInfo, object) ResolveMethod(string methodName)
		{
			var tokens = methodName.Split('.');
			object target;
			MethodInfo method;

			if (tokens.Length == 1)
			{
				method = GetType().GetMethod(methodName);
				target = this;
			}
			else
			{
				var property = ResolveProperty(this, tokens.SkipLast(1).ToArray());
				var propertyValue = property.Item1.GetValue(property.Item2);

				if (propertyValue == null)
					throw new RuntimeException($"{SR.ErrPropertyValueNull} ({property.Item1.Name})");

				method = propertyValue.GetType().GetMethod(tokens.Last());
				target = propertyValue;
			}

			if (method == null)
				throw new RuntimeException($"{SR.ErrMethodNotFound} ({tokens[tokens.Length - 1]})");

			return (method, target);
		}

		private (PropertyInfo, object) ResolveProperty(object instance, string[] tokens)
		{
			var property = instance.GetType().GetProperty(tokens[0]);

			if (property == null)
				throw new RuntimeException($"{SR.ErrPropertyNotFound} ({tokens[0]})");

			if (tokens.Length == 1)
				return (property, instance);

			var value = property.GetValue(instance);

			if (value == null)
				throw new RuntimeException($"{SR.ErrPropertyValueNull} ({tokens[0]})");

			return ResolveProperty(value, tokens.Skip(1).ToArray());
		}
	}
}