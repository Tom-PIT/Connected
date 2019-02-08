using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
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

		public static string DesignView(this IIoTElement stencil)
		{
			return stencil.GetType().FindAttribute<IoTElementAttribute>().DesignView;
		}

		public static StencilModel CreateModel(this IIoTElement stencil, IExecutionContext context)
		{
			var att = stencil.GetType().FindAttribute<IoTElementAttribute>();

			return att.Model.CreateInstance<StencilModel>(new object[] { context, stencil });
		}

		public static int Right(this IIoTElement element)
		{
			return element.Left + element.Width;
		}

		public static int Bottom(this IIoTElement element)
		{
			return element.Top + element.Height;
		}

		public static IIoTSchema ResolveSchema(this IIoTHub hub, IExecutionContext context)
		{
			if (string.IsNullOrWhiteSpace(hub.Schema))
				return null;

			var ms = hub.MicroService(context.Connection());
			var schema = hub.Schema;

			if (schema.Contains('/'))
			{
				var tokens = schema.Split('/');
				var originMicroService = context.Connection().GetService<IMicroServiceService>().Select(ms);
				schema = tokens[1];

				originMicroService.ValidateMicroServiceReference(context.Connection(), tokens[0]);

				var microService = context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new RuntimeException(SR.ErrMicroServiceNotFound).WithMetrics(context);

				ms = microService.Token;
			}

			return context.Connection().GetService<IComponentService>().SelectConfiguration(ms, "IoTSchema", schema) as IIoTSchema;
		}
	}
}
