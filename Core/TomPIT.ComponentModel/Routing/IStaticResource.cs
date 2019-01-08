using TomPIT.ComponentModel;

namespace TomPIT.Routing
{
	public interface IStaticResource : IElement
	{
		string VirtualPath { get; }
	}
}
