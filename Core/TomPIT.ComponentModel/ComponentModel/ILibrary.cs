namespace TomPIT.ComponentModel
{
	public interface ILibrary : IConfiguration
	{
		ListItems<IText> Scripts { get; }
	}
}
