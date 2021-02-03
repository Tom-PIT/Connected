using System;

namespace TomPIT.Design
{
	public interface IDeployment
	{
		void Deploy(string remote, Guid repository);
		void Deploy(IPullRequest request);
	}
}
