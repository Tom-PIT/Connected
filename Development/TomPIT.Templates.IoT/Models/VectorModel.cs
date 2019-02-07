using TomPIT.IoT.UI.Stencils;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public abstract class VectorModel<T> : StencilTypedModel<T> where T : IIoTElement
	{
		public VectorModel(IExecutionContext context, IIoTElement element) : base(context, element)
		{
		}
	}
}
