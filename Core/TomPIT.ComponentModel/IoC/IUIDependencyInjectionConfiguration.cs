using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IUIDependencyInjectionConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IUIDependency> Injections { get; }
	}
}
