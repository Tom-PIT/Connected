using System;

namespace TomPIT.Cdn;

public interface IEventDescriptor
{
    Guid Identifier { get; }
    string Name { get; }
    DateTime Created { get; }
    string Arguments { get; }
    string Callback { get; }
    Guid MicroService { get; }
}
