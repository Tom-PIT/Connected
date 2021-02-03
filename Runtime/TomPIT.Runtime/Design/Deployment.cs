using System;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class Deployment : TenantObject, IDeployment
	{
		public Deployment(ITenant tenant) : base(tenant)
		{
		}

		public void Deploy(string remote, Guid repository)
		{
			throw new NotImplementedException();
		}

		public void Deploy(IPullRequest request)
		{
			new DeploymentSession(Tenant, request).Deploy();
		}
	}
}
