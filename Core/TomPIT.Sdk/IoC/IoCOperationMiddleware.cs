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

		protected List<IIoCEndpointMiddleware> CreateEndpoints(object e)
		{
			return Context.Tenant.GetService<IIoCService>().CreateEndpoints(Context, ((IIoCOperationContext)this).Operation, e);
		}

		protected List<IIoCEndpointMiddleware> CreateEndpoints()
		{
			return Context.Tenant.GetService<IIoCService>().CreateEndpoints(Context, ((IIoCOperationContext)this).Operation, this);
		}

		protected IIoCEndpointMiddleware FirstEndpoint()
		{
			var endpoints = CreateEndpoints();

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[0];
		}

		protected IIoCEndpointMiddleware FirstEndpoint(object e)
		{
			var endpoints = CreateEndpoints(e);

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

		protected IIoCEndpointMiddleware LastEndpoint(object e)
		{
			var endpoints = CreateEndpoints(e);

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[^1];
		}

		protected bool HasEndpoints(object e)
		{
			return Context.Tenant.GetService<IIoCService>().HasEndpoints(Context, ((IIoCOperationContext)this).Operation, e);
		}

		protected bool HasEndpoints()
		{
			return Context.Tenant.GetService<IIoCService>().HasEndpoints(Context, ((IIoCOperationContext)this).Operation, this);
		}

		protected void Invoke(IIoCEndpointMiddleware endpoint, object e)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCEndpointMiddleware<object>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<object>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = Context.Tenant.GetService<ICompilerService>().CreateInstance<object>(Context as IMicroServiceContext, parameters[0].ParameterType, Serializer.Serialize(e));

			method.Invoke(endpoint, new object[] { parameter });
		}
		protected void Invoke(IIoCEndpointMiddleware endpoint)
		{
			Invoke(endpoint, this);
		}

		protected void Invoke(List<IIoCEndpointMiddleware> endpoints)
		{
			Parallel.ForEach(endpoints,
				(i) =>
				{
					Invoke(i);
				});
		}

		protected T Invoke<T>(List<IIoCEndpointMiddleware> endpoints)
		{
			var result = new List<T>();

			Parallel.ForEach(endpoints,
				(i) =>
				{
					var r = Invoke<T>(i);

					if (r != null)
					{
						lock (result)
						{
							result.Add(r);
						}
					}
				});

			if (result.Count == 0)
				return default;

			if (typeof(T) is IList)
			{
				var instance = typeof(T).CreateInstance() as IList;

				foreach (var item in result)
				{
					if (item is IList list)
					{
						foreach (var subItem in list)
							instance.Add(subItem);
					}
				}

				return (T)instance;
			}

			return result.Last();
		}

		protected T Invoke<T>(IIoCEndpointMiddleware endpoint, object e)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCOperationMiddleware<object>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<object>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = CreateInstance(parameters[0].ParameterType, e);

			var result = method.Invoke(endpoint, new object[] { parameter });

			if (result == null)
				return default;

			if (typeof(T).IsTypePrimitive())
			{
				if (result == default)
					return default;

				return (T)result;
			}

			return (T)CreateInstance(typeof(T), result);

		}
		protected T Invoke<T>(IIoCEndpointMiddleware endpoint)
		{
			return Invoke<T>(endpoint, this);
		}

		private object CreateInstance(Type type, object arguments)
		{
			var instance = TypeExtensions.CreateInstance(type);

			if (instance == null)
				return default;

			ReflectionExtensions.SetPropertyValue(instance, nameof(IMiddlewareObject.Context), Context);

			if (arguments != null && !type.IsTypePrimitive())
				Serializer.Populate(arguments, instance);

			return instance;
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
			return Invoke<R>(CreateEndpoints());
		}
	}
}
