using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestExtenderSupportedType : IManifestReturnType
	{
		List<string> Extenders { get; }
	}
}
