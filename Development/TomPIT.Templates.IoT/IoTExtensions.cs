using TomPIT.IoT.Annotations;
using TomPIT.IoT.Models;
using TomPIT.IoT.UI.Stencils;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public static class IoTExtensions
	{
		public static string View(this IIoTElement stencil)
		{
			return stencil.GetType().FindAttribute<IoTElementAttribute>().View;
		}

		public static StencilModel CreateModel(this IIoTElement stencil, IExecutionContext context)
		{
			var att = stencil.GetType().FindAttribute<IoTElementAttribute>();

			return att.Model.CreateInstance<StencilModel>(new object[] { context, stencil });
		}
	}
}
