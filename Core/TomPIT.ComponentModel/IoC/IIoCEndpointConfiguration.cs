namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCEndpointConfiguration : IConfiguration, IText
	{
		string Container { get; }
	}
}
