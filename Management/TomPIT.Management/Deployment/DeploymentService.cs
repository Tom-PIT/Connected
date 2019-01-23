using TomPIT.Connectivity;

namespace TomPIT.Deployment
{
	internal class DeploymentService : IDeploymentService
	{
		public DeploymentService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

	}
}
