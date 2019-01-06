namespace TomPIT.ComponentModel
{
	public interface ILibrary : IConfiguration
	{
		ListItems<ITemplate> Scripts { get; }
	}
}
