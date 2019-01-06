namespace TomPIT.SysDb.Development
{
	public interface IDevelopmentHandler
	{
		IMicroServiceHandler MicroServices { get; }
		IFeatureHandler Features { get; }
		IComponentHandler Components { get; }
		IQaHandler QA { get; }
	}
}
