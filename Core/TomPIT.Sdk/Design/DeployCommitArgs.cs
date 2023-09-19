using System;

namespace TomPIT.Design
{
	public class DeployCommitArgs
	{
		public bool Enabled { get; set; } = true;
		public string Comment { get; set; }
		public Guid Id { get; set; }
	}
}
