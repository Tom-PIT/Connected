using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data
{
	internal class InstallState : IInstallState
	{
		public Guid Package { get; set; }
		public Guid Parent { get; set; }
		public InstallStateStatus Status { get; set; }
	}
}
