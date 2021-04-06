using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IScriptManifestType : IScriptManifestMember
	{
		List<IScriptManifestMember> Members { get; }
	}
}
