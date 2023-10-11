using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Exceptions;
using TomPIT.MicroServices.IoT.Annotations;
using TomPIT.MicroServices.IoT.Models;
using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.IoT
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

		public static StencilModel CreateModel(this IIoTElement stencil, IMiddlewareContext context)
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

		public static IIoTHubConfiguration ResolveHub(this IIoTViewConfiguration view, IMiddlewareContext context)
		{
			if (string.IsNullOrWhiteSpace(view.Hub))
				return null;

			var ms = view.Configuration().MicroService();
			var hub = view.Hub;

			if (hub.Contains('/'))
			{
				var tokens = hub.Split('/');
				var originMicroService = context.Tenant.GetService<IMicroServiceService>().Select(ms);
				hub = tokens[1];

				originMicroService.ValidateMicroServiceReference(tokens[0]);

				var microService = context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new RuntimeException(SR.ErrMicroServiceNotFound);

				ms = microService.Token;
			}

			return context.Tenant.GetService<IComponentService>().SelectConfiguration(ms, ComponentCategories.IoTHub, hub) as IIoTHubConfiguration;
		}
	}
}
