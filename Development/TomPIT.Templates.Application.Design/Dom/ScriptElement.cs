using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;

namespace TomPIT.MicroServices.Design.Dom
{
	internal class ScriptElement : ComponentElement
	{
		public ScriptElement(IDomElement parent, IComponent component) : base(parent, component)
		{
			SetGlyph();
		}

		private IScriptConfiguration Config => Component as IScriptConfiguration;

		private void SetGlyph()
		{
			if (Config.Scope == ElementScope.Public)
				Glyph = "fal fa-file-code text-success";
			else
				Glyph = "fal fa-file-code text-secondary";
		}
	}
}