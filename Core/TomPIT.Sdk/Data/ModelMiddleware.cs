using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class ModelMiddleware<T> : MiddlewareComponent, IModelMiddleware<T>
	{
		private IComponent _component = null;
		private IModelConfiguration _configuration = null;
		public void Execute(string operation)
		{
			throw new System.NotImplementedException();
		}

		public R Execute<R>(string operation)
		{
			throw new System.NotImplementedException();
		}

		public void Execute(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		public R Execute<R>(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		public List<T> Query(string operation)
		{
			var op = ResolveOperation(operation);

			throw new System.NotImplementedException();
		}

		public List<R> Query<R>(string operation)
		{
			throw new System.NotImplementedException();
		}

		public List<T> Query(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		public List<R> Query<R>(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		public T Select(string operation)
		{
			throw new System.NotImplementedException();
		}

		public R Select<R>(string operation)
		{
			throw new System.NotImplementedException();
		}

		public T Select(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		public R Select<R>(string operation, object e)
		{
			throw new System.NotImplementedException();
		}

		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

				return _component;
			}
		}

		private IModelConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Context.Tenant.GetService<IComponentService>().SelectConfiguration(Component.Token) as IModelConfiguration;

				return _configuration;
			}
		}

		private IModelOperation ResolveOperation(string operation)
		{
			SyncEntity();

			var op = Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, operation, true) == 0);

			if (op == null)
				throw Context.Services.Diagnostic.Exception($"{SR.ErrModelOperationNotFound} ({Component.Name}/{operation})");

			return op;
		}

		private void SyncEntity()
		{
			Context.Tenant.GetService<IModelService>().SynchronizeEntity(Configuration);
		}
	}
}
