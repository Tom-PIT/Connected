using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Data
{
	public interface IUniqueValueProvider
	{
		List<object> ProvideUniqueValues(IDataModelContext context, string propertyName);
		bool IsUnique(IDataModelContext context, string propertyName);
	}
}
