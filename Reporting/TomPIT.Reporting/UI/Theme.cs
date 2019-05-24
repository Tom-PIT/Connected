using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	public class Theme : ComponentConfiguration, ITheme
	{
		private ListItems<IThemeFile> _stylesheets = null;

		public const string ComponentCategory = "Theme";

		[Items("TomPIT.Reporting.Design.Items.StylesheetCollection, TomPIT.Reporting.Design")]
		public ListItems<IThemeFile> Stylesheets
		{
			get
			{
				if (_stylesheets == null)
					_stylesheets = new ListItems<IThemeFile> { Parent = this };

				return _stylesheets;
			}
		}
	}
}
