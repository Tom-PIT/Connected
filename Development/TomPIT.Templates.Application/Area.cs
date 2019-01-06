using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	[Create("Area")]
	[DomElement("TomPIT.Dom.AreaElement, TomPIT.Application.Design")]
	public class Area : ComponentConfiguration
	{
		public const string ComponentCategory = "Area";
	}
}
