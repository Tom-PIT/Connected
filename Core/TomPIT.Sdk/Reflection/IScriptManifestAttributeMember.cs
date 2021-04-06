using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IScriptManifestAttributeMember
	{
		List<IScriptManifestAttribute> Attributes { get; }
	}
}
