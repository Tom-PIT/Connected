using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCContainerConfiguration : IConfiguration
	{
		ListItems<IIoCOperation> Operations { get; }
	}
}
