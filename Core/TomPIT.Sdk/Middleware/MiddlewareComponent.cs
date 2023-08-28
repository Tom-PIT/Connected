using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Data;
using TomPIT.Exceptions;

namespace TomPIT.Middleware
{
    public abstract class MiddlewareComponent : MiddlewareObject, IMiddlewareComponent
    {
        private MiddlewareValidator? _validator = null;
        protected MiddlewareComponent()
        {

        }

        protected MiddlewareComponent(IMiddlewareContext context) : base(context)
        {
        }

        protected virtual void OnValidate(List<ValidationResult> results)
        {

        }

        [Obsolete("Please use async method")]
        protected virtual List<object>? OnProvideUniqueValues(string propertyName)
        {
            return AsyncUtils.RunSync(() => OnProvideUniqueValuesAsync(propertyName));
        }

        protected virtual async Task<List<object>?> OnProvideUniqueValuesAsync(string propertyName)
        {
            await Task.CompletedTask;

            return default;
        }

        bool IUniqueValueProvider.IsUnique(IMiddlewareContext context, string propertyName)
        {
            return IsValueUnique(propertyName);
        }

        async Task<bool> IUniqueValueProvider.IsUniqueAsync(IMiddlewareContext context, string propertyName)
        {
            return await IsValueUniqueAsync(propertyName);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [Obsolete("Please use async method")]
        protected virtual bool IsValueUnique(string propertyName)
        {
            return AsyncUtils.RunSync(() => IsValueUniqueAsync(propertyName));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual async Task<bool> IsValueUniqueAsync(string propertyName)
        {
            await Task.CompletedTask;

            return true;
        }

        [Obsolete("Please Use Async version.")]
        public void Validate()
        {
            AsyncUtils.RunSync(ValidateAsync);
        }

        public async Task ValidateAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please Use Async version.")]
        protected void Validate(object instance)
        {
            AsyncUtils.RunSync(() => ValidateAsync(instance));
        }

        protected async Task ValidateAsync(object instance)
        {
            await Validator.Validate(instance, false);
        }

        [SkipValidation]
        [JsonIgnore]
        private MiddlewareValidator Validator
        {
            get
            {
                if (_validator is null)
                {
                    _validator = new MiddlewareValidator(this);

                    _validator.Validating += OnValidating;
                }

                return _validator;
            }
        }

        private void OnValidating(object sender, List<ValidationResult> results)
        {
            try
            {
                OnValidate(results);
            }
            catch (Exception ex)
            {
                throw TomPITException.Unwrap(this, ex);
            }
        }

        protected override void OnDisposing()
        {
            if (_validator is not null)
            {
                _validator.Validating -= OnValidating;
                _validator.Dispose();
                _validator = null;
            }

            base.OnDisposing();
        }
    }
}
