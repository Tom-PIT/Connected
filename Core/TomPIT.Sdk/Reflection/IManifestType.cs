using System.Collections.Generic;

namespace TomPIT.Reflection
{
	public interface IManifestType : IManifestMember
	{
		List<IManifestMember> Members { get; }
	}
}