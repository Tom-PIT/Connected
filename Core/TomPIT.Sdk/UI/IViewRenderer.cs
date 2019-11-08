using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.UI
{
	public interface IViewRenderer
	{
		string CreateContent(ITenant tenant, IComponent component);
	}
}
