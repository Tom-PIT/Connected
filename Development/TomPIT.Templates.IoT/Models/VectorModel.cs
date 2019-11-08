using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.IoT.Models
{
	public abstract class VectorModel<T> : StencilTypedModel<T> where T : IIoTElement
	{
		public VectorModel(IMiddlewareContext context, IIoTElement element) : base(context, element)
		{
		}
	}
}
