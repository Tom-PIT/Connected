using TomPIT.Annotations;
using TomPIT.IoT.Annotations;

namespace TomPIT.IoT.UI.Stencils.Shapes
{
	[IoTElement(typeof(RectangleModel), "~/Views/IoT/Stencils/Rectangle.cshtml", "~/Views/Ide/Designers/IoT/Stencils/Rectangle.cshtml")]
	[ToolboxItemGlyph("~/Views/Ide/Designers/IoT/Stencils/RectangleGlyph.cshtml")]
	[ComponentCreatingHandler("TomPIT.IoT.UI.Stencils.StencilCreateHandler, TomPIT.IoT")]
	public class Rectangle : IoTElement
	{
	}
}
