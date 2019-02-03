namespace TomPIT.ComponentModel
{
	public interface IServiceReference : IConfigurationElement
	{
		string MicroService { get; }
	}
}
