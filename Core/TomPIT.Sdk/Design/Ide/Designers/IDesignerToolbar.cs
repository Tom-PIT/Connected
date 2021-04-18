using System.Collections.Generic;

namespace TomPIT.Design.Ide.Designers
{
	public interface IDesignerToolbar : IEnvironmentObject
	{
		List<IDesignerToolbarAction> Items { get; }
	}
}
