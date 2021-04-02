using TomPIT.Annotations;

namespace TomPIT.Reflection
{
	public interface IManifestHttpType : IManifestType
	{
		HttpVerbs Verbs { get; }
	}
}
