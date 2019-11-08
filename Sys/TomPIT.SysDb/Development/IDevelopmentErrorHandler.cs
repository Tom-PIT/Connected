using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.SysDb.Development
{
	public interface IDevelopmentErrorHandler
	{
		IDevelopmentComponentError Select(Guid identifier);
		List<IDevelopmentComponentError> Query(IMicroService microService, ErrorCategory category);
		void Clear(IComponent component, Guid element, ErrorCategory category);
		void Delete(Guid identifier);
		void Insert(IMicroService microService, IComponent component, List<IDevelopmentError> errors);
	}
}
