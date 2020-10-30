using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface IAmbientProvider
	{
		public List<IAmbientToolbarAction> ToolbarActions { get; }
	}
}
