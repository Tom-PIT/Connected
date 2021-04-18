using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestAttributeMember
	{
		List<IManifestAttribute> Attributes { get; }
	}
}
