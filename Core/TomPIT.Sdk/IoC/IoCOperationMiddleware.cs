using System;
using System.Collections.Generic;
using TomPIT.Compilation;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.IoC
{
	public abstract class IoCOperationMiddleware : MiddlewareComponent, IIoCOperationMiddleware
	{
		protected List<IIoCEndpointMiddleware> CreateEndpoints<A>(A e)
		{
			return Context.Tenant.GetService<IIoCService>().CreateEndpoints(this, e);
		}

		protected IIoCEndpointMiddleware FirstEndpoint<A>(A e)
		{
			var endpoints = CreateEndpoints(e);

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[0];
		}

		protected IIoCEndpointMiddleware LastEndpoint<A>(A e)
		{
			var endpoints = CreateEndpoints(e);

			if (endpoints == null || endpoints.Count == 0)
				return null;

			return endpoints[^1];
		}

		protected bool HasEndpoints<A>(A e)
		{
			return Context.Tenant.GetService<IIoCService>().HasEndpoints(this, e);
		}
	}

	public abstract class IoCOperationMiddleware<A> : IoCOperationMiddleware, IIoCOperationMiddleware<A>
	{
		public void Invoke(A e)
		{
			Invoke(e, CreateEndpoints(e));
		}

		public void Invoke(A e, List<IIoCEndpointMiddleware> endpoints)
		{
			foreach (var endpoint in endpoints)
				Invoke(e, endpoint);
		}
		public void Invoke(A e, IIoCEndpointMiddleware endpoint)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCOperationMiddleware<A>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<A>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = Context.Tenant.GetService<ICompilerService>().CreateInstance<object>(Context as IMicroServiceContext, parameters[0].ParameterType, Serializer.Serialize(e));

			method.Invoke(endpoint, new object[] { parameter });
		}
	}

	public abstract class IoCOperationMiddleware<R, A> : IoCOperationMiddleware, IIoCContainerMiddleware<R, A>
	{
		public List<R> Invoke(A e)
		{
			return Invoke(e, CreateEndpoints(e));
		}

		public List<R> Invoke(A e, List<IIoCEndpointMiddleware> endpoints)
		{
			var result = new List<R>();

			foreach (var endpoint in endpoints)
				result.Add(Invoke(e, endpoint));

			return result;
		}
		public R Invoke(A e, IIoCEndpointMiddleware endpoint)
		{
			var method = endpoint.GetType().GetMethod(nameof(IIoCOperationMiddleware<A>.Invoke));

			if (method == null)
				throw new RuntimeException($"{SR.ErrIoCMethodExpected} ({nameof(IIoCOperationMiddleware<A>.Invoke)}");

			var parameters = method.GetParameters();
			var parameter = CreateInstance(parameters[0].ParameterType, e);

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
