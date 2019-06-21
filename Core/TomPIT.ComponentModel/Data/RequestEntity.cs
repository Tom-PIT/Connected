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
			OnValidating(results);
		}

		protected virtual void OnValidating(List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(string propertyName)
		{
			return null;
		}

		bool IUniqueValueProvider.IsUnique(IDataModelContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}
	}
}
