using System.Collections.Generic;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers
{
	public interface IDesignerToolbar : IEnvironmentObject
	{
		List<IDesignerToolbarAction> Items { get; }
	}
}
