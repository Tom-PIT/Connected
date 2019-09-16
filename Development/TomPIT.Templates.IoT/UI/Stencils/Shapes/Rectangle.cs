using TomPIT.Annotations.Design;
using TomPIT.MicroServices.IoT.Annotations;

namespace TomPIT.MicroServices.IoT.UI.Stencils.Shapes
{
	[IoTElement(typeof(RectangleModel), "~/Views/IoT/Stencils/Rectangle.cshtml", "~/Views/Ide/Designers/IoT/Stencils/Rectangle.cshtml")]
	[ToolboxItemGlyph("~/Views/Ide/Designers/IoT/Stencils/RectangleGlyph.cshtml")]
	[ComponentCreatingHandler("TomPIT.MicroServices.IoT.UI.Stencils.StencilCreateHandler, TomPIT.MicroServices.IoT")]
	public class Rectangle : IoTElement
	{
	}
}
