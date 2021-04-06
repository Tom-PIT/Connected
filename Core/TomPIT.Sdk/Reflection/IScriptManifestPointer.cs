using System;

namespace TomPIT.Reflection
{
	public interface IScriptManifestPointer : IEquatable<IScriptManifestPointer>
	{
		short Id { get; }
		Guid MicroService { get;  }
		Guid Component { get; }
		Guid Element { get; }
	}
}
