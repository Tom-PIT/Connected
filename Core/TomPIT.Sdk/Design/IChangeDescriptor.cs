using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface IChangeDescriptor
	{
		List<IChangeMicroService> MicroServices { get; }
	}
}
