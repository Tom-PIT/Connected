using TomPIT.Annotations;
using TomPIT.IoT.Annotations;

namespace TomPIT.IoT.UI.Stencils.Shapes
{
	[IoTElement(typeof(TextModel), "~/Views/IoT/Stencils/Text.cshtml", "~/Views/Ide/Designers/IoT/Stencils/Text.cshtml")]
	[ToolboxItemGlyph("~/Views/Ide/Designers/IoT/Stencils/TextGlyph.cshtml")]
	[ComponentCreatingHandler("TomPIT.IoT.UI.Stencils.StencilCreateHandler, TomPIT.IoT")]
	public class Text : IoTElement
	{
		public string String { get; set; }
	}
}
