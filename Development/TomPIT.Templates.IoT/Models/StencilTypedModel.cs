using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.IoT.Models
{
	public abstract class StencilTypedModel<T> : StencilModel where T : IIoTElement
	{
		public StencilTypedModel(IMiddlewareContext context, IIoTElement element) : base(context, element)
		{
		}

		public T Stencil { get { return (T)Element; } }
	}
}
