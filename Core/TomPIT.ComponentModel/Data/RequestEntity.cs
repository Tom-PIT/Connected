using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Data
{
	public abstract class RequestEntity: IUniqueValueProvider
	{
		public void Validate(IDataModelContext context, List<ValidationResult> results)
		{
			OnValidating(context, results);
		}

		protected virtual void OnValidating(IDataModelContext context, List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(IDataModelContext context, string propertyName)
		{
			return null;
		}

		List<object> IUniqueValueProvider.ProvideUniqueValues(IDataModelContext context, string propertyName)
		{
			return OnProvideUniqueValues(context, propertyName);
		}
	}
}
