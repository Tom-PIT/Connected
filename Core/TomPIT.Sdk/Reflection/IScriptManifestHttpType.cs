using TomPIT.Annotations;

namespace TomPIT.Reflection
{
	public interface IScriptManifestHttpType : IScriptManifestType
	{
		HttpVerbs Verbs { get; }
	}
}
