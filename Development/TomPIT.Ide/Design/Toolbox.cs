using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	internal class Toolbox : EnvironmentClient, IDesignerToolbox
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
