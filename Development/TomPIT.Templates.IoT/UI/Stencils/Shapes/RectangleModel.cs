using TomPIT.IoT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.UI.Stencils.Shapes
{
	public class RectangleModel : VectorModel<Rectangle>
	{
		public RectangleModel(IExecutionContext context, IIoTElement element) : base(context, element)
		{
		}
	}
}
