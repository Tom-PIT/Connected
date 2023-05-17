using System;
using System.Collections.Immutable;

namespace TomPIT.Design
{
	public interface IDesignService
	{
		IDeployment Deployment { get; }

		IVersionControl VersionControl { get; }
		IComponentModel Components { get; }
		[Obsolete]
		IDesignSearch Search { get; }
		[Obsolete]
		ITextDiff TextDiff { get; }
		IMicroServiceDesign MicroServices { get; }

		void Initialize();

		ImmutableList<string> QueryDesigners();
	}
}
