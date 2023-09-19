using System;
using System.Collections.Generic;
using TomPIT.Connectivity;
using TomPIT.Deployment;

namespace TomPIT.Ide.ComponentModel
{
	internal class MicroServiceDevelopmentService : TenantObject, IMicroServiceDevelopmentService
	{
		public MicroServiceDevelopmentService(ITenant tenant) : base(tenant)
		{
		}

		public void DeleteString(Guid microService, Guid element, string property)
		{
		}

		public void RestoreStrings(Guid microService, List<IPackageString> strings)
		{
		}

		public void UpdateString(Guid microService, Guid language, Guid element, string property, string value)
		{
		}
	}
}
