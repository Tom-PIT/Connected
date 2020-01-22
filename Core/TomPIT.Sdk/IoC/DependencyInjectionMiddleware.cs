using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.IoC
{
	public abstract class DependencyInjectionObject : MiddlewareObject, IDependencyInjectionObject
	{
		private IMiddlewareOperation _operation = null;

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

		public void Synchronize(object instance)
		{
			if (instance != null)
				Serializer.Populate(instance, this);
		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

		}
	}

	public class DependencyInjectionMiddleware : DependencyInjectionObject, IDependencyInjectionMiddleware
	{
		public void Invoke(object e)
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
