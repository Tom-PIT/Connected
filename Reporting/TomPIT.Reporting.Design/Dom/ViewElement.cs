using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Reporting.Security;
using TomPIT.Security;

namespace TomPIT.Reporting.Design.Dom
{
	internal class ViewElement : ComponentElement
	{
		public ViewElement(IDomElement parent, IComponent component) : base(parent, component)
		{
		}
	}
}
