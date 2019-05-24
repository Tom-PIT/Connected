using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[DomDesigner("TomPIT.Application.Design.Designers.StringTable, TomPIT.Application.Design")]
	public class StringTable : ComponentConfiguration, IStringTable
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
