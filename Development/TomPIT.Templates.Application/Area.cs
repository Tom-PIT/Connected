using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	[Create("Area")]
	[DomElement("TomPIT.Application.Design.Dom.AreaElement, TomPIT.Application.Design")]
	public class Area : ComponentConfiguration
	{
		public const string ComponentCategory = "Area";
	}
}
