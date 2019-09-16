using System.Collections.Generic;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers.Toolbar
{
	public class Dropdown : ToolbarAction
	{
		private List<IDesignerToolbarAction> _items = null;

		public Dropdown(IEnvironment environment) : base(environment)
		{
			View = "~/Views/Ide/Actions/Dropdown.cshtml";
			Group = "Default";
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
