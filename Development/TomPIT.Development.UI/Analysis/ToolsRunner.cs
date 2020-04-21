using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Analysis;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Ide.Analysis;

namespace TomPIT.Development.Analysis
{
	internal class ToolsRunner : HostedService
	{
		public ToolsRunner()
		{
			IntervalTimeout = TimeSpan.FromSeconds(1);
		}
		protected override Task Process(CancellationToken cancel)
		{
			var tenants = Shell.GetService<IConnectivityService>()?.QueryTenants();

			if (tenants == null)
				return Task.CompletedTask;

			Parallel.ForEach(tenants,
				 (i) =>
				 {
					 RunTools(cancel, Shell.GetService<IConnectivityService>().SelectTenant(i.Url));
				 });


			return Task.CompletedTask;

		}

		private void RunTools(CancellationToken cancel, ITenant tenant)
		{
			var tools = tenant.GetService<IToolsService>().Query().Where(f => f.Status == TomPIT.Analysis.ToolStatus.Pending);

			if (cancel.IsCancellationRequested)
				return;

			if (tools.Count() == 0)
				return;

			Parallel.ForEach(tools,
				(f) =>
				{
					if (cancel.IsCancellationRequested)
						return;

					RunTool(tenant, f);
				});
		}

		private void RunTool(ITenant tenant, ITool tool)
		{
			try
			{
				var instance = tenant.GetService<IToolsService>().GetTool(tool.Name);

				tenant.GetService<IToolsService>().Activate(tool.Name);

				if (instance != null)
					instance.Execute(tenant);
			}
			finally
			{
				tenant.GetService<IToolsService>().Complete(tool.Name);
			}
		}
	}
}
