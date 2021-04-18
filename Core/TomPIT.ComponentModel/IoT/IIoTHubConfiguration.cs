namespace TomPIT.ComponentModel.IoT
{
	public interface IIoTHubConfiguration : IConfiguration, IText, INamespaceElement
	{
		ElementScope Scope { get; }
	}
}
