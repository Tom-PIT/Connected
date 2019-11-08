using System;
using System.Collections.Generic;

namespace TomPIT.Ide.UI.Theming
{
	public interface IStylesheetService
	{
		List<IStylesheetClass> QueryClasses(Guid microService, bool includeDependencies);
	}
}
