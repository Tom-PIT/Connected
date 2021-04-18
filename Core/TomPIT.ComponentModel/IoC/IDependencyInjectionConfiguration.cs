using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IDependencyInjectionConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IDependency> Injections { get; }
	}
}
