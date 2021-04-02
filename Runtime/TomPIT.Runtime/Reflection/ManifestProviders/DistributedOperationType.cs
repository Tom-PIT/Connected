using TomPIT.Annotations;

namespace TomPIT.Reflection.ManifestProviders
{
	internal class DistributedOperationType : ManifestType, IManifestHttpType, IManifestDistributedType
	{
		public bool IsDistributed {get;set;}

		public HttpVerbs Verbs {get;set;}
	}
}
