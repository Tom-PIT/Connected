using System;

namespace TomPIT.Design
{
	public class DeployArgs : EventArgs
	{
		private DeployCommitArgs _commit;

		public bool ResetMicroService { get; set; }
		public DeployCommitArgs Commit => _commit ??= new DeployCommitArgs();
		
	}
}
