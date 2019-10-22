using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel.IoC;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.IoC
{
	public abstract class IoCOperationMiddlewareBase : MiddlewareComponent, IIoCOperationContext
	{
		IIoCOperation IIoCOperationContext.Operation { get; set; }

		protected List<IIoCEndpointMiddleware> CreateEndpoints()
		{
			return Context.Tenant.GetService<IIoCService>().CreateEndpoints(((IIoCOperationContext)this).Operation, this);
		}

		protected IIoCEndpointMiddleware FirstEndpoint<A>(A e)
		{
			var endpoints = CreateEndpoints();

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[0];
		}

		protected IIoCEndpointMiddleware LastEndpoint()
		{
			var endpoints = CreateEndpoints();

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[^1];
		}

		protected bool HasEndpoints()
		{
			return Context.Tenant.GetService<IIoCService>().HasEndpoints(((IIoCOperationContext)this).Operation, this);
		}
	}

	public abstract class IoCOperationMiddleware : IoCOperationMiddlewareBase, IIoCOperationMiddleware
	{
		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{
			Invoke(CreateEndpoints());
		}

		protected void Invoke(IIoCEndpointMiddleware endpoint)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCEndpointMiddleware<object>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<object>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = Context.Tenant.GetService<ICompilerService>().CreateInstance<object>(Context as IMicroServiceContext, parameters[0].ParameterType, Serializer.Serialize(this));

			method.Invoke(endpoint, new object[] { parameter });
		}

		protected void Invoke(List<IIoCEndpointMiddleware> endpoints)
		{
			Parallel.ForEach(endpoints,
				(i) =>
				{
					Invoke(i);
				});
		}
	}

	public abstract class IoCOperationMiddleware<R> : IoCOperationMiddlewareBase, IIoCOperationMiddleware<R>
	{
		public R Invoke()
		{
			Validate();

			return OnInvoke();
		}

		protected virtual R OnInvoke()
		{
			return Invoke(CreateEndpoints());
		}

		protected R Invoke(List<IIoCEndpointMiddleware> endpoints)
		{
			var result = new List<R>();

			Parallel.ForEach(endpoints,
				(i) =>
				{
					var r = Invoke(i);

					if (r != default)
					{
						lock (result)
						{
							result.Add(r);
						}
					}
				});

			if (result.Count == 0)
				return default;

			if (typeof(R) is IList)
			{
				var instance = typeof(R).CreateInstance() as IList;

				foreach (var item in result)
				{
					if (item is IList list)
					{
						foreach (var subItem in list)
							instance.Add(subItem);
					}
				}

				return (R)instance;
			}

			return result.Last();
		}

		protected R Invoke(IIoCEndpointMiddleware endpoint)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCOperationMiddleware<object>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<object>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = CreateInstance(parameters[0].ParameterType, this);

			var result = method.Invoke(endpoint, new object[] { parameter });

			if (result == null)
				return default;

			if (typeof(R).IsPrimitive)
			{
				if (result == default)
					return default;

				return (R)result;
			}

			return (R)CreateInstance(typeof(R), result);
		}

		private object CreateInstance(Type type, object arguments)
		{
			var instance = TypeExtensions.CreateInstance(type);

			if (instance == null)
				return default;

			ReflectionExtensions.SetPropertyValue(instance, nameof(IMiddlewareObject.Context), Context);

			if (arguments != null)
				Serializer.Populate(arguments, instance);

			return instance;
		}
	}
}
