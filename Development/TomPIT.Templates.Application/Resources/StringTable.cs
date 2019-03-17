using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	public class StringTable : ComponentConfiguration, IStringTable
	{
		private ListItems<IStringResource> _strings = null;

		[Items("TomPIT.Application.Design.Items.StringResourceCollection, TomPIT.Application.Design")]
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
