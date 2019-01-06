using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("ScriptBundle")]
	public class ScriptBundle : ComponentConfiguration, IScriptBundle
	{
		public const string ComponentCategory = "Bundle";

		private ListItems<IScriptSource> _scripts = null;

		[Items("TomPIT.Application.Items.ScriptSourceCollection, TomPIT.Templates.Application")]
		public ListItems<IScriptSource> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IScriptSource> { Parent = this };

				return _scripts;
			}
		}

		[DefaultValue(true)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool Minify { get; set; } = true;
	}
}

