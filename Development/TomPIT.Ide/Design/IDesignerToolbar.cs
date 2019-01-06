using System.Collections.Generic;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IDesignerToolbar : IEnvironmentClient
	{
		List<IDesignerToolbarAction> Items { get; }
	}
}
