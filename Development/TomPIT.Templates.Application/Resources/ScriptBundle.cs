using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.ScriptBundle)]
	[ComponentCreatedHandler(DesignUtils.ScriptBundleHandler)]
	[DomElement(DesignUtils.ScriptBundleElement)]
	[FileNameExtension("jsm")]
	public class ScriptBundle : ComponentConfiguration, IScriptBundleConfiguration, IScriptBundleInitializer
	{
		private ListItems<IScriptSource> _scripts = null;

		[Items(DesignUtils.ScriptSourceItems)]
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

		public IScriptSource CreateDefaultFile()
		{
			return new ScriptCodeSource
			{
				Name = "Default"
			};
		}
	}
}

