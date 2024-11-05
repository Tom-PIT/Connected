using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

		protected virtual List<object>? OnProvideUniqueValues(string propertyName)
		{
			return default;
		}

		bool IUniqueValueProvider.IsUnique(IMiddlewareContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}

		public void Validate()
		{
			AsyncUtils.RunSync(() => Validator.Validate());
		}

		protected void Validate(object instance)
		{
			AsyncUtils.RunSync(() => Validator.Validate(instance, false));
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
