using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.UI
{
	public interface IPartialView : IConfiguration, IGraphicInterface, ISnippetView
	{
		IServerEvent Invoke { get; }
	}
}
