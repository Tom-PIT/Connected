using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Data;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareComponent : MiddlewareObject, IMiddlewareComponent
	{
		private MiddlewareValidator _validator = null;
		public MiddlewareComponent()
		{

		}

		public MiddlewareComponent(IMiddlewareContext context) : base(context)
		{
		}

		protected virtual void OnValidate(List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(string propertyName)
		{
			return null;
		}

		bool IUniqueValueProvider.IsUnique(IMiddlewareContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}

		public void Validate()
		{
			Validator.Validate();
		}

		protected void Validate(object instance)
		{
			Validator.Validate(instance, false);
		}

		[SkipValidation]
		private MiddlewareValidator Validator
		{
			get
			{
				if (_validator == null)
				{
					_validator = new MiddlewareValidator(this);
					_validator.SetContext(Context);

					_validator.Validating += OnValidating;
				}

				return _validator;
			}
		}

		private void OnValidating(object sender, List<ValidationResult> results)
		{
			OnValidate(results);
		}
	}
}
