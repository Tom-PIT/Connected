using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Data;
using TomPIT.Security;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareComponent : MiddlewareObject, IMiddlewareComponent, IElevationContext
	{
		private MiddlewareValidator _validator = null;
		private List<string> _claims = null;
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
		[EditorBrowsable(EditorBrowsableState.Advanced)]
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

		List<string> IElevationContext.Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>();

				return _claims;
			}
		}

		private void OnValidating(object sender, List<ValidationResult> results)
		{
			OnValidate(results);
		}

		protected void Elevate(string claim)
		{
			var ctx = this as IElevationContext;

			if (!ctx.Claims.Contains(claim, StringComparer.OrdinalIgnoreCase))
				ctx.Claims.Add(claim);
		}
	}
}
