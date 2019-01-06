namespace TomPIT.Runtime.Services
{

	public interface IRuntimeService
	{
		IFeatureService Features { get; }
		IConfigurationService Configurations { get; }
		IComponentService Components { get; }
	}
}
