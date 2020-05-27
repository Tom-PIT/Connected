using System;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Reflection;

namespace TomPIT.IoC
{
	[Obsolete]
	public abstract class DependencyInjectionObject : MiddlewareObject, IApiDependencyInjectionObject
	{
		private IMiddlewareOperation _operation = null;

		protected DependencyInjectionObject()
		{
			if (Shell.HttpContext == null || Shell.HttpContext.Items["RootModel"] == null)
				return;

			if (Shell.HttpContext.Items["RootModel"] is IRuntimeModel model)
				BindRequestValues(model);
		}

		private void BindRequestValues(IRuntimeModel model)
		{
			if (model.Arguments == null)
				return;

			var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				var att = property.FindAttribute<RequestArgumentAttribute>();

				if (att == null || string.IsNullOrWhiteSpace(att.PropertyName))
					continue;

				var value = model.Arguments.Optional<object>(att.PropertyName, null);

				if (value == null)
					continue;

				property.SetValue(this, Types.Convert(value, property.PropertyType));
			}
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

	[Obsolete]
	public class DependencyInjectionMiddleware : DependencyInjectionObject, IApiDependencyInjectionMiddleware
	{
		public void Invoke(object e)
		{
			OnInvoke(e);
		}

		protected virtual void OnInvoke(object e)
		{

		}
	}
	[Obsolete]
	public class DependencyInjectionMiddleware<T> : DependencyInjectionObject, IApiDependencyInjectionMiddleware<T>
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
