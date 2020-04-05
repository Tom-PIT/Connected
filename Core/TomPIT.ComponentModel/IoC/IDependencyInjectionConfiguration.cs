using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IDependencyInjectionConfiguration : IConfiguration
	{
		ListItems<IDependency> Injections { get; }
	}
}
