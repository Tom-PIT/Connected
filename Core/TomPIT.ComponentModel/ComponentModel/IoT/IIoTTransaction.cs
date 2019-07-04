using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTTransaction : IConfigurationElement, ISourceCode
	{
		string Name { get; }
	}
}
