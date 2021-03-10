namespace TomPIT.Reflection
{
	public interface IMicroServiceDiscovery
	{
		IMicroServiceReferencesDiscovery References { get; }
		IMicroServiceInfoDiscovery Info { get; }
	}
}
