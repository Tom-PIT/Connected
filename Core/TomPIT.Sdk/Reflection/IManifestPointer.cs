using System;

namespace TomPIT.Reflection
{
	public interface IManifestPointer
	{
		short Id { get; }
		Guid MicroService { get;  }
		Guid Component { get; }
		Guid Element { get; }
	}
}
