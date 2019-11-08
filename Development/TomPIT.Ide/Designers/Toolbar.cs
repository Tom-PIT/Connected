using System.Collections.Generic;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers
{
	internal class DesignerToolbar : EnvironmentObject, IDesignerToolbar
	{
		private List<IDesignerToolbarAction> _items = null;

		public DesignerToolbar(IEnvironment environment) : base(environment)
		{
		}

		public List<IDesignerToolbarAction> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IDesignerToolbarAction>();

				return _items;
			}
		}
	}
}
