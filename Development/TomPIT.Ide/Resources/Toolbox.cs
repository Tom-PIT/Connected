using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Resources
{
	internal class Toolbox : EnvironmentObject, IToolbox
	{
		private List<IItemDescriptor> _items = null;

		public Toolbox(IEnvironment environment) : base(environment)
		{
		}

		public List<IItemDescriptor> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IItemDescriptor>();

				return _items;
			}
		}
	}
}
