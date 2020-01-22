using System;
using System.Collections.Generic;
using TomPIT.Compilation;
using TomPIT.Exceptions;
using TomPIT.IoC;
using TomPIT.Reflection;
using TomPIT.Serialization;
using TomPIT.UI;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation, IMiddlewareTransactionClient, ISerializationStateProvider
	{
		private List<IDependencyInjectionObject> _dependencies = null;
		protected MiddlewareOperation()
		{
		}

		protected MiddlewareOperation(IMiddlewareTransaction transaction)
		{
			AttachTransaction(transaction);
		}

		protected IMiddlewareTransaction Transaction { get; private set; }
		object ISerializationStateProvider.SerializationState { get; set; }

		internal void AttachTransaction(IMiddlewareOperation sender)
		{
			if (sender is MiddlewareOperation o)
				AttachTransaction(o.Transaction);
		}

		private void AttachTransaction(IMiddlewareTransaction transaction)
		{
			Transaction = transaction;

			if (transaction is MiddlewareTransaction t)
				t.Notify(this);

			IsCommitable = false;
		}

		public IMiddlewareTransaction Begin()
		{
			if (Transaction != null)
				return Transaction;

			Transaction = new MiddlewareTransaction(Context)
			{
				Id = Guid.NewGuid()
			};

			return Transaction;
		}

		protected bool IsCommitable { get; private set; } = true;

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
					var ms = Context.Tenant.GetService<ICompilerService>().ResolveMicroService(this);

					_dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QueryApiDependencies($"{ms.Name}/{component.Name}/{GetType().ShortName()}", this);

					if (_dependencies != null)
					{
						foreach (var dependency in _dependencies)
						{
							ReflectionExtensions.SetPropertyValue(dependency, nameof(IDependencyInjectionObject.Operation), this);
							dependency.Synchronize(((ISerializationStateProvider)this).SerializationState);
						}
					}
					else
						_dependencies = new List<IDependencyInjectionObject>();
				}

				return _dependencies;
			}
		}
	}
}
