using System.Collections.Generic;
using TomPIT.Ide.Messaging;

namespace TomPIT.Ide.Environment.Providers
{
	public interface IEventProvider : IEnvironmentObject
	{
		List<IEvent> Events { get; }
	}
}
