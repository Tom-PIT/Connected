namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResources : IConfiguration
	{
		ListItems<IMediaResourceFolder> Folders { get; }
		ListItems<IMediaResourceFile> Files { get; }
	}
}
