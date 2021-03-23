using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom.ComponentModel;

namespace TomPIT.MicroServices.Design.Dom
{
	internal class ScriptBundleElement : ComponentElement
	{
		private IDomDesigner _designer = null;
		public ScriptBundleElement(IDomElement parent, IComponent component) : base(parent, component)
		{
		}

		private IScriptBundleConfiguration Configuration => Component as IScriptBundleConfiguration;

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
				{
					if (Configuration.Scripts.OfType<IScriptCodeSource>().Count() == 0)
						return base.Designer;

					Element.LoadChildren();

					var target = Element.Items.FirstOrDefault(f => f.Component == Component);

					if (target == null)
						return base.Designer;

					target.LoadChildren();

					foreach (var child in target.Items)
					{
						if (child.Component is IScriptCodeSource code)
						{
							_designer = new TextDesigner(child);
							break;
						}
					}
				}

				return _designer == null ? base.Designer : _designer;
			}
		}
	}
}
