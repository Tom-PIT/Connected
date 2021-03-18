using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation<TReturnValue> : MiddlewareApiOperation, IOperation<TReturnValue>
	{
		protected Operation()
		{

		}

		[CIP(CIP.ExtenderProvider)]
		public string Extender { get; set; }

		public T Invoke<T>()
		{
			return Invoke<T>(null);
		}
		public T Invoke<T>(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			var r = Invoke();

			if (r == null)
				return default;

			if (r.GetType().IsCollection())
			{
				var listResult = (IList)r;
				var genericArguments = typeof(T).GenericTypeArguments;
				var result = (IList)typeof(List<>).MakeGenericType(genericArguments).CreateInstance();

				foreach (var item in listResult)
					result.Add(item);

				return (T)result;
			}
			else
				return (T)Convert.ChangeType(r, typeof(T));
		}

		public TReturnValue Invoke()
		{
			return Invoke(null);
		}
		public TReturnValue Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			ValidateExtender();
			Validate();
			OnValidating();

			var metrics = StartMetrics();
			var success = true;

			try
			{
				if (Context.Environment.IsInteractive)
				{
					AuthorizePolicies();
					OnAuthorize();
					OnAuthorizing();
				}

				var result = DependencyInjections.Invoke(OnInvoke());

				if (result != null && !string.IsNullOrWhiteSpace(Extender))
					result = Extend(result);

				Invoked();

				return result;
			}
			catch (ValidationException)
			{
				success = false;
				Rollback();

				throw;
			}
			catch (Exception ex)
			{
				success = false;
				Rollback();

				throw TomPITException.Unwrap(this, ex);
			}
			finally
			{
				StopMetrics(metrics, success, null);
			}
		}

		private TReturnValue Extend(TReturnValue items)
		{
			var ext = ResolveExtenderType();
			using var ctx = new MicroServiceContext(Context.Tenant.GetService<ICompilerService>().ResolveMicroService(this), Context);
			var extenderInstance = Context.Tenant.GetService<ICompilerService>().CreateInstance<object>(ctx, ext);
			var inputType = GetExtendingType(extenderInstance);
			var list = typeof(List<>);
			var genericList = list.MakeGenericType(new Type[] { inputType });
			var method = extenderInstance.GetType().GetMethod("ExtendAsync", new Type[] { genericList });

			if (method == null)
				method = extenderInstance.GetType().GetMethod("Extend", new Type[] { genericList });

			if (items.GetType().IsCollection())
			{
				var listResult = (IList)items;
				var extenderResult = method.Invoke(extenderInstance, new object[] { items }) as IList;

				listResult.Clear();

				foreach (var i in extenderResult)
					listResult.Add(i);

				if (Context.Environment.IsInteractive)
					return DependencyInjections.Authorize(OnAuthorize(AuthorizePolicies(items)));
				else
					return items;
			}
			else
			{
				var listInstance = (IList)genericList.CreateInstance();

				listInstance.Add(items);

				var listResult = method.Invoke(extenderInstance, new object[] { listInstance }) as IList;

				if (Context.Environment.IsInteractive)
					return DependencyInjections.Authorize(OnAuthorize(AuthorizePolicies((TReturnValue)listResult[0])));
				else
					return (TReturnValue)listResult[0];
			}
		}

		private Type GetExtendingType(object extenderInstance)
		{
			var interfaces = extenderInstance.GetType().GetInterfaces();

			foreach (var i in interfaces)
			{
				if (i.IsGenericType && string.Compare(i.Name, typeof(IExtender<object, object>).Name, false) == 0)
					return i.GetGenericArguments()[0];
			}

			throw new RuntimeException($"{SR.ErrCannotResolveExtender} ({GetType().ShortName()})");
		}
		protected virtual TReturnValue OnInvoke()
		{
			return default;
		}

		protected virtual TReturnValue OnAuthorize(TReturnValue e)
		{
			return e;
		}

		protected virtual void OnAuthorize()
		{

		}

		private void ValidateExtender()
		{
			if (string.IsNullOrWhiteSpace(Extender))
				return;

			ResolveExtenderType();
		}

		private Type ResolveExtenderType()
		{
			var extenders = GetType().FindAttributes<ExtenderAttribute>();

			if (extenders == null)
				throw new RuntimeException($"{SR.ErrExtenderNotSupported} ({Extender})");

			var extender = extenders.FirstOrDefault(f => string.Compare(f.Extender.ShortName(), Extender, true) == 0);

			if (extender == null)
				throw new RuntimeException($"{SR.ErrExtenderNotSupported} ({Extender})");

			return extender.Extender;
		}

		private TReturnValue AuthorizePolicies(TReturnValue e)
		{
			if (e == null)
				return e;

			var attributes = GetType().GetCustomAttributes(true);
			var targets = new List<AuthorizationPolicyAttribute>();

			foreach (var attribute in attributes)
			{
				if (!(attribute is AuthorizationPolicyAttribute policy) || policy.MiddlewareStage != AuthorizationMiddlewareStage.Result)
					continue;

				targets.Add(policy);
			}

			if (typeof(TReturnValue).IsCollection())
			{
				var items = (IList)e;
				var result = typeof(TReturnValue).CreateInstance<IList>();

				foreach (var item in items)
				{
					var authorized = AuthorizePoliciesItem(item, targets);

					if (authorized != default)
						result.Add(authorized);
				}

				return (TReturnValue)result;
			}
			else
				return (TReturnValue)AuthorizePoliciesItem(e, targets);
		}

		private object AuthorizePoliciesItem(object e, List<AuthorizationPolicyAttribute> attributes)
		{
			if (attributes.Count == 0)
				return e;

			var restore = false;

			if (Context is IElevationContext c && c.State == ElevationContextState.Granted)
			{
				restore = true;
				c.State = ElevationContextState.Revoked;
			}

			Exception firstFail = null;
			bool onePassed = false;

			try
			{
				foreach (var attribute in attributes.OrderByDescending(f => f.Priority))
				{
					try
					{
						if (attribute.Behavior == AuthorizationPolicyBehavior.Optional && onePassed)
							continue;

						attribute.Authorize(Context, e);

						onePassed = true;
					}
					catch (Exception ex)
					{
						if (attribute.Behavior == AuthorizationPolicyBehavior.Mandatory)
							throw;

						firstFail = ex;
					}
				}

				if (!onePassed && firstFail != null)
					throw firstFail;

				return e;
			}
			finally
			{
				if (restore && Context is IElevationContext elevation)
					elevation.State = ElevationContextState.Granted;
			}
		}
	}
}
