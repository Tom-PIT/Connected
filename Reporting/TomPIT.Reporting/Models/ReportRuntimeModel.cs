using TomPIT.ComponentModel;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.Reporting.Models
{
	public class ReportRuntimeModel : MicroServiceContext
	{
		public ReportRuntimeModel(IMicroServiceContext sender, string url) : base(sender)
		{
			ReportUrl = url;

			var tokens = ReportUrl.Split('/');

			if (tokens.Length == 1)
				MicroServiceName = MicroService.Name;
			else
			{
				var ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					throw new RuntimeException($"SR.ErrMicroServiceNotFound ({tokens[0]})").WithMetrics(this);

				MicroService.ValidateMicroServiceReference(ms.Name);

				MicroServiceName = ms.Name;
			}

			ReportName = tokens[1];
		}

		public string ReportUrl { get; internal set; }

		public string MicroServiceName { get; }
		public string ReportName { get; }
	}
}
