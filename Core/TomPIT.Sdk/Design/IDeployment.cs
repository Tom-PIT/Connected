using System;
using System.Collections.Immutable;

namespace TomPIT.Design
{
	public interface IDeployment
	{
		public ImmutableArray<Guid> DeployingMicroServices { get; }
		void Deploy(string remote, Guid repository, long branch, long commit, string authenticationToken);
		void Deploy(IPullRequest request, DeployArgs e);

		IDeploymentConfiguration Configuration { get; }
	}
}
