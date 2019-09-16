using System;
using System.Collections.Generic;
using TomPIT.Design.Tools;
using TomPIT.Development;
using TomPIT.Ide.Properties;

namespace TomPIT.Ide.Designers
{
	public interface IDesignerService
	{
		IPropertyEditor GetPropertyEditor(string name);

		void RegisterPropertyEditor(string name, string view);

		IDevelopmentComponentError SelectError(Guid identifier);
		List<IDevelopmentComponentError> QueryErrors();
		List<IDevelopmentComponentError> QueryErrors(Guid microService);
		List<IDevelopmentComponentError> QueryErrors(Guid microService, ErrorCategory category);
		List<IDevelopmentComponentError> QueryErrors(ErrorCategory category);

		void ClearErrors(Guid component, Guid element, ErrorCategory category);
		void InsertErrors(Guid component, List<IDevelopmentError> errors);

		void RegisterAutoFix(IAutoFixProvider provider);
		void AutoFix(string provider, Guid error);
		List<IAutoFixProvider> QueryAutoFixProviders();
	}
}
