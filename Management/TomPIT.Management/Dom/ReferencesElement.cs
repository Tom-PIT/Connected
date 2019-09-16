using TomPIT.ComponentModel;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;

namespace TomPIT.Management.Dom
{
	internal class ReferencesElement : ComponentElement
	{
		public ReferencesElement(IDomElement parent, IComponent component) : base(parent, component)
		{
			((Behavior)Behavior).Static = true;

			Glyph = "fas fa-tilde";
			Verbs.Clear();
		}
	}
}
