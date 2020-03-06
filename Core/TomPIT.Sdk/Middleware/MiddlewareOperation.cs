using System;
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
		//private IMiddlewareTransaction _transaction = null;
		protected MiddlewareOperation()
		{
		}

		protected MiddlewareOperation(IMiddlewareTransaction transaction)
		{
			AttachTransaction(transaction);
		}

		protected IMiddlewareTransaction Transaction { get; set; }
		//{
		//	get
		//	{
		//		if (_transaction == null)
		//		{
		//			if (Context is MiddlewareContext ctx)
		//			{
		//				if (ctx.Transaction != null)
		//					AttachTransaction(ctx.Transaction);
		//				else if (ctx.Owner?.Transaction != null)
		//					AttachTransaction(ctx.Owner.Transaction);
		//			}
		//		}

		//		return _transaction;
		//	}
		//	private set { _transaction = value; }
		//}

		internal void AttachTransaction(IMiddlewareOperation sender)
		{
			if (sender is MiddlewareOperation o)
				AttachTransaction(o.Transaction);
		}

		internal void AttachTransaction(IMiddlewareTransaction transaction)
		{
			Transaction = transaction;

			if (transaction is MiddlewareTransaction t)
				t.Notify(this);

			if (Context is MiddlewareContext m)
				m.Transaction = transaction;

			IsCommitable = false;
		}

		public IMiddlewareTransaction Begin()
		{
			if (Transaction != null)
				return Transaction;

			var transaction = new MiddlewareTransaction(Context)
			{
				Id = Guid.NewGuid()
			};

			if (Context is MiddlewareContext m)
				m.Transaction = transaction;

			Transaction = transaction;

			return Transaction;
		}

		protected bool IsCommitable { get; private set; } = true;
		protected internal bool IsCommitted { get; set; }

		protected void RenderPartial(string partialName)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			engine.Context = Shell.HttpContext;
			engine.RenderPartial(Context as IMicroServiceContext, partialName);
		}

		protected void Commit()
		{
			if (!IsCommitable)
				return;

			if (Transaction is MiddlewareTransaction t)
				t.Commit();
		}

		protected void Rollback()
		{
			if (!IsCommitable)
				return;

			if (Transaction is MiddlewareTransaction t)
				t.Rollback();
		}

		void IMiddlewareTransactionClient.CommitTransaction()
		{
			if (IsCommitted)
				return;

			IsCommitted = true;
			OnCommit();
		}

		void IMiddlewareTransactionClient.RollbackTransaction()
		{
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
	}
}
