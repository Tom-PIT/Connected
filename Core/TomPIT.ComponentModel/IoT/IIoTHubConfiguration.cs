namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTHubConfiguration : IConfiguration, IText
	{
		ElementScope Scope { get; }
	}
}
