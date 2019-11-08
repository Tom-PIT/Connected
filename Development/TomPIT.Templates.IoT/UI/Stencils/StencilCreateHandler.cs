using TomPIT.Design;
using TomPIT.MicroServices.IoT.UI.Stencils.Shapes;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.IoT.UI.Stencils
{
	internal class StencilCreateHandler : IComponentCreateHandler
	{
		public void InitializeNewComponent(IMiddlewareContext context, object instance)
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
