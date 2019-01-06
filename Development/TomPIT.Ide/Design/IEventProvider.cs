using System.Collections.Generic;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IEventProvider : IEnvironmentClient
	{
		List<IEvent> Events { get; }
	}
}
