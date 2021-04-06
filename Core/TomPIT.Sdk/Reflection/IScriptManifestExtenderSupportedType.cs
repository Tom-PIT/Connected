using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IScriptManifestExtenderSupportedType : IScriptManifestReturnType
	{
		List<string> Extenders { get; }
	}
}
