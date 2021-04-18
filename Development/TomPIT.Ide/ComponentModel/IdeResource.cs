using TomPIT.Design.Ide;

namespace TomPIT.Ide.ComponentModel
{
	public class IdeResource : IIdeResource
	{
		public IdeResourceType Type { get; set; }

		public string Path { get; set; }
	}
}
