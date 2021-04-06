using System;

namespace TomPIT.Reflection
{
	public interface IScriptReferenceDescriptor
	{
		Guid MicroService { get; }
		Guid Component { get; }
		Guid Element { get; }

		IScriptManifestSymbolLocation Location { get; }
	}
}
