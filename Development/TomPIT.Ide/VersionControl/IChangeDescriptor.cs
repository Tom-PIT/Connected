using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeDescriptor
	{
		List<IChangeMicroService> MicroServices { get; }
	}
}
