using TomPIT.Collections;

namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResourcesConfiguration : IConfiguration
	{
		ListItems<IMediaResourceFolder> Folders { get; }
		ListItems<IMediaResourceFile> Files { get; }
	}
}
