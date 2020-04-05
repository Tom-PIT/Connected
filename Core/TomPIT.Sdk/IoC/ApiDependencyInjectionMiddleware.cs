using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.IoC
{
	public abstract class ApiDependencyInjectionObject : MiddlewareObject, IApiDependencyInjectionObject
	{
		private IMiddlewareOperation _operation = null;

		protected ApiDependencyInjectionObject()
		{
			if (Shell.HttpContext == null || Shell.HttpContext.Items["RootModel"] == null)
				return;

			if (Shell.HttpContext.Items["RootModel"] is IRuntimeModel model)
				Serializer.Populate(model.Arguments, this);
		}

		public IMiddlewareOperation Operation
		{
			get
			{
				return _operation;
			}
			private set
			{
				_operation = value;

				if (_operation != null)
					ReflectionExtensions.SetPropertyValue(this, nameof(Context), _operation.Context);
			}
		}

		public void Authorize()
		{
			OnAuthorize();
		}

		protected virtual void OnAuthorize()
		{

		}

		public void Validate()
		{
			OnValidate();
		}

		protected virtual void OnValidate()
		{

		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

		}

		public void Rollback()
		{
			OnRollback();
		}

		protected virtual void OnRollback()
		{

		}
	}

	public class ApiDependencyInjectionMiddleware : ApiDependencyInjectionObject, IApiDependencyInjectionMiddleware
	{
		public void Invoke(object e)
		{
			OnInvoke(e);
		}

		protected virtual void OnInvoke(object e)
		{

		}
	}

	public class ApiDependencyInjectionMiddleware<T> : ApiDependencyInjectionObject, IApiDependencyInjectionMiddleware<T>
	{
		public T Authorize(T e)
		{
			return OnAuthorize(e);
		}

		protected virtual T OnAuthorize(T e)
		{
			return e;
		}

		public T Invoke(T e)
		{
			return OnInvoke(e);
		}

		protected virtual T OnInvoke(T e)
		{
			return default;
		}
	}
}
