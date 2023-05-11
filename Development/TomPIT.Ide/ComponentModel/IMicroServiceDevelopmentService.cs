using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Ide.ComponentModel
{
	[Obsolete]
	public interface IMicroServiceDevelopmentService
	{
		void UpdateString(Guid microService, Guid language, Guid element, string property, string value);
		void DeleteString(Guid microService, Guid element, string property);
		void RestoreStrings(Guid microService, List<IPackageString> strings);
	}
}
