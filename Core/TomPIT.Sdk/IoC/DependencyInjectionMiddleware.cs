using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public abstract class DependencyInjectionObject : MiddlewareObject, IDependencyInjectionObject
	{
		public IMiddlewareOperation Operation { get; private set; }

		public void Authorize()
		{
			OnAuthorize();
		}

		protected virtual void OnAuthorize()
		{

		}

		public void Validate(List<ValidationResult> results)
		{
			OnValidate(results);
		}

		protected virtual void OnValidate(List<ValidationResult> results)
		{

		}
	}

	public class DependencyInjectionMiddleware : DependencyInjectionObject, IDependencyInjectionMiddleware
	{
		public void Invoke()
		{
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}

	public class DependencyInjectionMiddleware<T> : DependencyInjectionObject, IDependencyInjectionMiddleware<T>
	{
		public T Authorize(T e)
		{
			return OnAuthorize(e);
		}

		protected virtual T OnAuthorize(T e)
		{
			return e;
		}

		public T Invoke()
		{
			return OnInvoke();
		}

		protected virtual T OnInvoke()
		{
			return default;
		}
	}
}
