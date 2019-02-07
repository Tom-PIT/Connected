using TomPIT.IoT.UI.Stencils;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public abstract class StencilModel
	{
		public StencilModel(IExecutionContext context, IIoTElement element)
		{
			Context = context;
			Element = element;
		}

		public IIoTElement Element { get; }
		public IExecutionContext Context { get; }

		public bool IsDesignTime { get { return (Shell.GetService<IRuntimeService>().Mode & EnvironmentMode.Design) == EnvironmentMode.Design; } }
	}
}
