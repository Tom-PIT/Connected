using TomPIT.Collections;

namespace TomPIT.ComponentModel
{
	public interface IServiceReferencesConfiguration : IConfiguration
	{
		ListItems<IServiceReference> MicroServices { get; }
		ListItems<IAssemblyReference> Assemblies { get; }
	}
}
