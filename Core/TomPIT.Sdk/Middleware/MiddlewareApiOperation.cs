using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.IoC;
using TomPIT.Reflection;

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
	}
}
