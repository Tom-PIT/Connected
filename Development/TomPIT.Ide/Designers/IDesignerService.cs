using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide.Properties;
using TomPIT.Design.Tools;
using TomPIT.Development;

namespace TomPIT.Ide.Designers
{
	public interface IDesignerService
	{
		IPropertyEditor GetPropertyEditor(string name);

		void RegisterPropertyEditor(string name, string view);
		[Obsolete]
		IDevelopmentComponentError SelectError(Guid identifier);
		[Obsolete]
		List<IDevelopmentComponentError> QueryErrors();
		[Obsolete]
		List<IDevelopmentComponentError> QueryErrors(Guid microService);
		[Obsolete]
		List<IDevelopmentComponentError> QueryErrors(Guid microService, ErrorCategory category);
		[Obsolete]
		List<IDevelopmentComponentError> QueryErrors(ErrorCategory category);
		[Obsolete]
		void ClearErrors(Guid component, Guid element, ErrorCategory category);
		[Obsolete]
		void InsertErrors(Guid component, List<IDevelopmentError> errors);
		[Obsolete]
		void RegisterAutoFix(IAutoFixProvider provider);
		[Obsolete]
		void AutoFix(string provider, Guid error);
		[Obsolete]
		List<IAutoFixProvider> QueryAutoFixProviders();
		[Obsolete]
		List<IComponentDevelopmentState> DequeueDevelopmentStates(int count);
	}
}
