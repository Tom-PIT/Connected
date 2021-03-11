using System;

namespace TomPIT.Design
{
	public interface IDeployment
	{
		void Deploy(string remote, Guid repository, string userName, string password);
		void Deploy(IPullRequest request);
	}
}
