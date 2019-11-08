using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class InstallState : IInstallState
	{
		public Guid Package { get; set; }
		public Guid Parent { get; set; }
		public InstallStateStatus Status { get; set; }
		public DateTime Modified { get; set; }
		public string Error { get; set; }
	}
}
