using System;
using System.Reflection;
using System.Threading.Tasks;
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
            AsyncUtils.RunSync(AuthorizeAsync);
        }

        public async Task AuthorizeAsync()
        {
            OnAuthorize();

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnAuthorize()
        {
            AsyncUtils.RunSync(OnAuthorizeAsync);
        }

        protected virtual async Task OnAuthorizeAsync()
        {
            await Task.CompletedTask;
        }

        public void Validate()
        {
            AsyncUtils.RunSync(ValidateAsync);
        }

        public async Task ValidateAsync()
        {
            OnValidate();

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnValidate()
        {
            AsyncUtils.RunSync(OnValidateAsync);
        }

        protected virtual async Task OnValidateAsync()
        {

        }

        public void Commit()
        {
            AsyncUtils.RunSync(CommitAsync);
        }

        public async Task CommitAsync()
        {
            OnCommit();

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnCommit()
        {
            AsyncUtils.RunSync(OnCommitAsync);
        }

        protected virtual async Task OnCommitAsync()
        {
            await Task.CompletedTask;
        }

        public void Rollback()
        {
            AsyncUtils.RunSync(RollbackAsync);
        }

        public async Task RollbackAsync()
        {
            OnRollback();

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnRollback()
        {
            AsyncUtils.RunSync(OnRollbackAsync);
        }

        protected virtual async Task OnRollbackAsync()
        {
            await Task.CompletedTask;
        }
    }

    [Obsolete]
    public class DependencyInjectionMiddleware : DependencyInjectionObject, IApiDependencyInjectionMiddleware
    {
        public void Invoke(object e)
        {
            AsyncUtils.RunSync(() => InvokeAsync(e));
        }

        public async Task InvokeAsync(object e)
        {
            OnInvoke(e);

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnInvoke(object e)
        {
            AsyncUtils.RunSync(() => OnInvokeAsync(e));
        }

        protected virtual async Task OnInvokeAsync(object e)
        {
            await Task.CompletedTask;
        }
    }
    [Obsolete]
    public class DependencyInjectionMiddleware<T> : DependencyInjectionObject, IApiDependencyInjectionMiddleware<T>
    {
        public T Authorize(T e)
        {
            return AsyncUtils.RunSync(() => AuthorizeAsync(e));
        }

        public async Task<T> AuthorizeAsync(T e)
        {
            await Task.CompletedTask;

            return await Task.FromResult(OnAuthorize(e));
        }

        [Obsolete("Please use async method")]
        protected virtual T OnAuthorize(T e)
        {
            return AsyncUtils.RunSync(() => OnAuthorizeAsync(e));
        }

        protected virtual async Task<T> OnAuthorizeAsync(T e)
        {
            return await Task.FromResult(e);
        }

        public T Invoke(T e)
        {
            return AsyncUtils.RunSync(() => InvokeAsync(e));
        }

        public async Task<T> InvokeAsync(T e)
        {
            return await Task.FromResult(OnInvoke(e));
        }

        protected virtual T OnInvoke(T e)
        {
            return AsyncUtils.RunSync(() => OnInvokeAsync(e));
        }

        protected virtual async Task<T> OnInvokeAsync(T e)
        {
            return await Task.FromResult(e);
        }
    }
}
