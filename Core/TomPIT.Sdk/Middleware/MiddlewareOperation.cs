using System.Collections.Generic;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.IoC;
using TomPIT.Reflection;
using TomPIT.UI;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation, IMiddlewareTransactionClient
	{
		private List<IDependencyInjectionObject> _dependencies = null;

		public IMiddlewareTransaction Transaction
		{
			get
			{
				if (Context is MiddlewareContext mc)
				{
					var transaction = mc.Transaction;

					if (transaction != null && transaction is MiddlewareTransaction mt)
						mt.Notify(this);

					return transaction;
				}

				return null;
			}
			internal set
			{
				if (Context is MiddlewareContext mc)
					mc.Transaction = value;

				if (value is MiddlewareTransaction transaction)
					transaction.Notify(this);
			}
		}

		protected void RenderPartial(string partialName)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			engine.Context = Shell.HttpContext;
			engine.RenderPartial(Context as IMicroServiceContext, partialName);
		}

		protected void Rollback()
		{
			if (Transaction is MiddlewareTransaction t)
				t.Rollback();
		}

		void IMiddlewareTransactionClient.CommitTransaction()
		{
			OnCommit();
			OnCommitDependencies();
		}

		void IMiddlewareTransactionClient.RollbackTransaction()
		{
			OnRollbackDependencies();
			OnRollback();
		}

		protected virtual void OnCommit()
		{

		}

		protected virtual void OnRollback()
		{

		}

		protected internal List<IDependencyInjectionObject> DependencyInjections
		{
			get
			{
				if (_dependencies == null)
				{
					var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

					if (component != null)
					{
						var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

						_dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QueryApiDependencies($"{ms.Name}/{component.Name}/{GetType().ShortName()}", this);
					}

					if (_dependencies != null)
					{
						foreach (var dependency in _dependencies)
							ReflectionExtensions.SetPropertyValue(dependency, nameof(IDependencyInjectionObject.Operation), this);
					}
					else
						_dependencies = new List<IDependencyInjectionObject>();
				}

				return _dependencies;
			}
		}

		protected internal void Invoked()
		{
			var mc = Context as MiddlewareContext;

			if (mc?.Owner == null)
			{
				if (!(Transaction is MiddlewareTransaction transaction))
					return;

				transaction.Commit();
			}
		}

		protected internal virtual void OnCommitDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Commit();
		}

		protected internal virtual void OnRollbackDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Rollback();
		}

		protected internal void OnAuthorizeDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Authorize();
		}

		protected internal void OnValidateDependencies()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Validate();
		}
	}
}
