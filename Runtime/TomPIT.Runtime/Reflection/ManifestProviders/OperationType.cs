using TomPIT.Annotations;

namespace TomPIT.Reflection.ManifestProviders
{
	internal class OperationType : ManifestType, IManifestHttpType
	{
		public HttpVerbs Verbs {get;set;}

	}
}
