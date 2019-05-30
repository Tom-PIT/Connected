using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Reporting.Models
{
	public class ReportRuntimeModel : ExecutionContext
	{
		public ReportRuntimeModel(IExecutionContext sender, string url) : base(sender)
		{
			ReportUrl = url;

			var tokens = ReportUrl.Split('/');

			if (tokens.Length == 1)
				MicroServiceName = MicroService.Name;
			else
			{
				var ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					throw new RuntimeException($"SR.ErrMicroServiceNotFound ({tokens[0]})").WithMetrics(this);

				MicroService.ValidateMicroServiceReference(Connection, ms.Name);

				MicroServiceName = ms.Name;
			}

			ReportName = tokens[1];
		}

		public string ReportUrl { get; internal set; }

		public string MicroServiceName { get; }
		public string ReportName { get; }
	}
}
