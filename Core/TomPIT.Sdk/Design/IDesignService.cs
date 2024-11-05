using System.Collections.Immutable;

namespace TomPIT.Design;

public interface IDesignService
{
	IDeployment Deployment { get; }
	IComponentModel Components { get; }
	IMicroServiceDesign MicroServices { get; }

	void Initialize();
	ImmutableList<string> QueryDesigners();
}
