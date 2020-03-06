using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation<TReturnValue> : MiddlewareOperation, IOperation<TReturnValue>
	{
		protected Operation()
		{

		}

		[CIP(CIP.ExtenderProvider)]
		public string Extender { get; set; }

		public T Invoke<T>()
		{
			var r = Invoke();

			if (r == default)
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
			var result = ProcessInvoke();

			//if (Context is MiddlewareContext mc)
			//	mc.CloseConnections();

			return result;
		}

		private TReturnValue ProcessInvoke()
		{
			ValidateExtender();
			Validate();
			ValidateDependencies();

			try
			{
				if (Context.Environment.IsInteractive)
				{
					OnAuthorize();
					AuthorizeDependencies();
				}

				var result = DependencyInjections.Invoke(OnInvoke());

				if (IsCommitable)
				{
					Commit();
					//OnCommit();
					CommitDependencies();
				}

				if (Context is MiddlewareContext mc)
					mc.CloseConnections();

				if (IsCommitable)
				{
					if (Transaction is MiddlewareTransaction t)
						t.Complete();
				}

				if (!IsCommitted)
					OnCommit();

				if (result == null)
					return result;

				if (string.IsNullOrWhiteSpace(Extender))
					return result;

				var ext = ResolveExtenderType();
				var ctx = new MicroServiceContext(Context.Tenant.GetService<ICompilerService>().ResolveMicroService(this));
				var extenderInstance = Context.Tenant.GetService<ICompilerService>().CreateInstance<object>(ctx, ext);
				var inputType = GetExtendingType(extenderInstance);
				var list = typeof(List<>);
				var genericList = list.MakeGenericType(new Type[] { inputType });
				var method = extenderInstance.GetType().GetMethod("ExtendAsync", new Type[] { genericList });

				if (method == null)
					method = extenderInstance.GetType().GetMethod("Extend", new Type[] { genericList });

				if (result.GetType().IsCollection())
				{
					var listResult = (IList)result;
					var extenderResult = method.Invoke(extenderInstance, new object[] { result }) as IList;

					listResult.Clear();

					foreach (var i in extenderResult)
						listResult.Add(i);

					if (Context.Environment.IsInteractive)
						return DependencyInjections.Authorize(OnAuthorize(result));
					else
						return result;
				}
				else
				{
					var listInstance = (IList)genericList.CreateInstance();

					listInstance.Add(result);

					var listResult = method.Invoke(extenderInstance, new object[] { listInstance }) as IList;

					if (Context.Environment.IsInteractive)
						return DependencyInjections.Authorize(OnAuthorize(((TReturnValue)listResult[0])));
					else
						return (TReturnValue)listResult[0];
				}
			}
			catch (Exception ex)
			{
				throw TomPITException.Unwrap(this, ex);
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

		protected internal void CommitDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Commit();
		}

		protected internal void AuthorizeDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Authorize();
		}

		protected internal void ValidateDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Validate();
		}
	}
}
