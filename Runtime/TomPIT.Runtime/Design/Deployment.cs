using System;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class Deployment : TenantObject, IDeployment
	{
		public Deployment(ITenant tenant) : base(tenant)
		{
		}

		public void Deploy(string remote, Guid repository, string userName, string password)
		{
			var url = $"{remote}/Repositories/IBranches/Pull";

			Deploy(Tenant.Post<PullRequest>(url, new
			{
				repository
			}, new HttpRequestArgs().WithBasicCredentials(userName, password)));
		}

		public void Deploy(IPullRequest request)
		{
			new DeploymentSession(Tenant, request).Deploy();
		}
	}
}
