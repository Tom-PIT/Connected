using TomPIT.Collections;

namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCContainerConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IIoCOperation> Operations { get; }
	}
}
