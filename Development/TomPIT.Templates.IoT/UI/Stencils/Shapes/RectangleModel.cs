using TomPIT.MicroServices.IoT.Models;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.IoT.UI.Stencils.Shapes
{
	public class RectangleModel : VectorModel<Rectangle>
	{
		public RectangleModel(IMiddlewareContext context, IIoTElement element) : base(context, element)
		{
		}
	}
}
