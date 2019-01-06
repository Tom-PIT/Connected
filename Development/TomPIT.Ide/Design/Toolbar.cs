using System.Collections.Generic;
using TomPIT.Ide;

namespace TomPIT.Design
{
	internal class Toolbar : EnvironmentClient, IDesignerToolbar
	{
		private List<IDesignerToolbarAction> _items = null;

		public Toolbar(IEnvironment environment) : base(environment)
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
