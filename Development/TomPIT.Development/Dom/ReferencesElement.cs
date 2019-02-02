using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Dom
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
