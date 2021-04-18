using TomPIT.Annotations;

namespace TomPIT.Reflection.ManifestProviders
{
	internal class OperationType : ScriptManifestType, IScriptManifestHttpType
	{
		public HttpVerbs Verbs {get;set;}

	}
}
