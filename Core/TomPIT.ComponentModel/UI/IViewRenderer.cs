using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.UI
{
	public interface IViewRenderer
	{
		string CreateContent(ISysConnection connection, IComponent component);
	}
}
