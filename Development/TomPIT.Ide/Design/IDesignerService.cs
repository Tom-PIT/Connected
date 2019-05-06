using System;
using System.Collections.Generic;
using TomPIT.Development;

namespace TomPIT.Design
{
	public interface IDesignerService
	{
		IPropertyEditor GetPropertyEditor(string name);

		void RegisterPropertyEditor(string name, string view);

		List<IDevelopmentError> QueryErrors(Guid microService);
		void ClearErrors(Guid component, Guid element);
		void InsertErrors(Guid component, List<IDevelopmentComponentError> errors);
	}
}
