using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public interface IDependencyInjectionObject : IMiddlewareObject
	{
		IMiddlewareOperation Operation { get; }
		void Validate(List<ValidationResult> results);
		void Authorize();

	}
	public interface IDependencyInjectionMiddleware : IDependencyInjectionObject
	{
		void Invoke();
	}

	public interface IDependencyInjectionMiddleware<T> : IDependencyInjectionObject
	{
		T Invoke();
		T Authorize(T e);
	}
}
