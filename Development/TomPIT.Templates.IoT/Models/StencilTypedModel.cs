using TomPIT.IoT.UI.Stencils;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public abstract class StencilTypedModel<T> : StencilModel where T : IIoTElement
	{
		public StencilTypedModel(IExecutionContext context, IIoTElement element) : base(context, element)
		{
		}

		public T Stencil { get { return (T)Element; } }
	}
}
