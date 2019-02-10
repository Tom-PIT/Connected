using TomPIT.Design;
using TomPIT.IoT.UI.Stencils.Shapes;
using TomPIT.Services;

namespace TomPIT.IoT.UI.Stencils
{
	internal class StencilCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IExecutionContext context, object instance)
		{
			if (instance is Rectangle r)
				InitializeRectangle(r);
			else if (instance is Text t)
				InitializeText(t);
		}

		private void InitializeText(Text t)
		{
			t.Css = "iot-text";
			t.String = "Text";
		}

		private void InitializeRectangle(Rectangle r)
		{
			r.Css = "iot-rect";
			r.Width = 100;
			r.Height = 100;
		}
	}
}
