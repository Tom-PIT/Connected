using System.Collections.Generic;

namespace TomPIT.Design.Ide.Events
{
	public interface IEventProvider : IEnvironmentObject
	{
		List<IEvent> Events { get; }
	}
}
