using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.SysDb.Development
{
	public interface IDevelopmentErrorHandler
	{
		List<IDevelopmentError> Query(IMicroService microService);
		void Clear(IComponent component, Guid element);
		void Insert(IMicroService microService, IComponent component, List<IDevelopmentComponentError> errors);
	}
}
