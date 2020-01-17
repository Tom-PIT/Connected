using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IUIDependencyInjectionConfiguration : IConfiguration
	{
		ListItems<IUIDependency> Injections { get; }
	}
}
