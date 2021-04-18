using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.IoC;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareApiOperation : MiddlewareOperation
	{
		private List<IApiDependencyInjectionObject> _dependencies = null;

		[SkipValidation]
		protected internal List<IApiDependencyInjectionObject> DependencyInjections
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
							ReflectionExtensions.SetPropertyValue(dependency, nameof(IApiDependencyInjectionObject.Operation), this);
					}
					else
						_dependencies = new List<IApiDependencyInjectionObject>();
				}

				return _dependencies;
			}
		}

		protected internal override void OnCommitting()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Commit();
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal override void OnRollbacking()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Rollback();
		}

		protected internal override void OnValidating()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Validate();
		}

		protected internal override void OnAuthorizing()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Authorize();
		}

		protected internal Guid StartMetrics()
		{
			if (Context.Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Production)
				return Guid.Empty;

			var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

			if (component == null)
				return Guid.Empty;

			var api = Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IApiConfiguration;

			if (api == null)
				return Guid.Empty;

			var op = api.Operations.FirstOrDefault(f => string.Compare(f.Name, GetType().ShortName(), false) == 0);

			if (op == null)
				return Guid.Empty;

			return Context.Services.Diagnostic.StartMetric(op.Metrics, op.Id, Serializer.Serialize(this));
		}

		protected internal void StopMetrics(Guid metric, bool success, object result)
		{
			if (metric == Guid.Empty)
				return;

			Context.Services.Diagnostic.StopMetric(metric, success ? Diagnostics.SessionResult.Success : Diagnostics.SessionResult.Fail, result);
		}
	}
}
