using TomPIT.ComponentModel.Messaging;

namespace TomPIT.ComponentModel.UI
{
	public interface IPartialViewConfiguration : IConfiguration, IGraphicInterface, ISnippetView
	{
		IServerEvent Invoke { get; }
	}
}
