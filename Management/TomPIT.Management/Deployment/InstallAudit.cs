﻿using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class InstallAudit : IInstallAudit
	{
		public Guid Package { get; set; }
		public InstallAuditType Type { get; set; }
		public DateTime Created { get; set; }
		public string Message { get; set; }
		public string Version { get; set; }
	}
}