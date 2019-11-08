using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[DomDesigner(DesignUtils.StringTableDesigner)]
	public class StringTable : ComponentConfiguration, IStringTableConfiguration
	{
		private ListItems<IStringResource> _strings = null;

		[Browsable(false)]
		public ListItems<IStringResource> Strings
		{
			get
			{
				if (_strings == null)
					_strings = new ListItems<IStringResource> { Parent = this };

				return _strings;
			}
		}
	}
}
