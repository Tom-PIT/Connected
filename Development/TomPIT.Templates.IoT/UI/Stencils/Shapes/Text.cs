using System.Collections.Generic;
using TomPIT.Annotations.Design;
using TomPIT.MicroServices.IoT.Annotations;

namespace TomPIT.MicroServices.IoT.UI.Stencils.Shapes
{
	[IoTElement(typeof(TextModel), "~/Views/IoT/Stencils/Text.cshtml", "~/Views/Ide/Designers/IoT/Stencils/Text.cshtml", Verbs = IoTDesignerVerbs.Move | IoTDesignerVerbs.Select)]
	[ToolboxItemGlyph("~/Views/Ide/Designers/IoT/Stencils/TextGlyph.cshtml")]
	[ComponentCreatingHandler("TomPIT.MicroServices.IoT.UI.Stencils.StencilCreateHandler, TomPIT.MicroServices.IoT")]
	[SuppressProperties("Width,Height")]
	public class Text : IoTElement
	{
		public Text()
		{
			Width = 0;
			Height = 0;
		}

		public string String { get; set; }
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.MicroServices.IoT.Design.Items.DataMemberItems, TomPIT.MicroServices.IoT.Design")]
		public string DataMember { get; set; }

		protected override void OnQueryBindings(List<IIoTBinding> items)
		{
			if (!string.IsNullOrWhiteSpace(DataMember))
			{
				items.Add(new IoTBinding
				{
					Field = DataMember
				});
			}

			base.OnQueryBindings(items);
		}
	}
}
