using System;

namespace TomPIT.Design;
public interface IDocumentIdentity
{
    public Guid MicroService { get; }
    public Guid Component { get; }
    public Guid Element { get; }
}
