﻿using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;

namespace TomPIT.Ide.Designers.Toolbar
{
	public class AddItems : ToolbarAction
	{
		private List<IDesignerToolbarAction> _items = null;

		public AddItems(IEnvironment environment) : base(environment)
		{
			View = "~/Views/Ide/Actions/AddItems.cshtml";
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
