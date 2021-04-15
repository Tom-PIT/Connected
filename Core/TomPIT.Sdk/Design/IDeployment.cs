using System;

namespace TomPIT.Design
{
	public interface IDeployment
	{
		void Deploy(string remote, Guid repository, string authenticationToken);
		void Deploy(IPullRequest request, DeployArgs e);
	}
}
