using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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
                if (_dependencies is null)
                {
                    var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

                    if (component is not null)
                    {
                        var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

                        _dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QueryApiDependencies($"{ms.Name}/{component.Name}/{GetType().ShortName()}", this);
                    }

                    if (_dependencies is not null)
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

        protected internal override async Task OnCommittingAsync()
        {
            foreach (var dependency in DependencyInjections)
                dependency.Commit();

            await Task.CompletedTask;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal override async Task OnRollbackingAsync()
        {
            foreach (var dependency in DependencyInjections)
                dependency.Rollback();

            await Task.CompletedTask;
        }

        protected internal override async Task OnValidatingAsync()
        {
            foreach (var dependency in DependencyInjections)
                dependency.Validate();

            await Task.CompletedTask;
        }

        protected internal override async Task OnAuthorizingAsync()
        {
            foreach (var dependency in DependencyInjections)
                dependency.Authorize();

            await Task.CompletedTask;
        }
    }
}
