namespace TomPIT.ComponentModel
{
	public interface IServiceReferences : IConfiguration
	{
		ListItems<IServiceReference> MicroServices { get; }
	}
}
