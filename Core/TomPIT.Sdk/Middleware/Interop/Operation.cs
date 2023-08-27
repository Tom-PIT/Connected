using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
    public abstract class Operation : MiddlewareApiOperation, IOperation
    {
        public void Invoke()
        {
            AsyncUtils.RunSync(() => InvokeAsync(null));
        }

        public async Task InvokeAsync()
        {
            await InvokeAsync(null);
        }

        public void Invoke(IMiddlewareContext? context)
        {
            AsyncUtils.RunSync(() => InvokeAsync(context));
        }

        public async Task InvokeAsync(IMiddlewareContext? context)
        {
            if (context is not null)
                this.WithContext(context);

            try
            {
                Validate();
                OnValidating();

                if (Context.Environment.IsInteractive)
                {
                    AuthorizePolicies();
                    OnAuthorize();
                    OnAuthorizing();
                }

                OnInvoke();
                DependencyInjections.Invoke<object>(null);

                Invoked();
            }
            catch (ValidationException)
            {
                Rollback();
                throw;
            }
            catch (Exception ex)
            {
                Rollback();

                throw TomPITException.Unwrap(this, ex);
            }

            await Task.CompletedTask;
        }

        protected virtual void OnInvoke()
        {
            AsyncUtils.RunSync(OnInvokeAsync);
        }

        protected virtual void OnAuthorize()
        {
            AsyncUtils.RunSync(OnAuthorizeAsync);
        }

        protected virtual async Task OnInvokeAsync()
        {
            await Task.CompletedTask;
        }

        protected virtual async Task OnAuthorizeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
