using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Events;

public interface IEventHandler
{
    void Update(ImmutableList<IEventDescriptor> events);
    List<IEventDescriptor> Query();
}
