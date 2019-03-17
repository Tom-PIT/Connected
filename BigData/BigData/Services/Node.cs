using System;

namespace TomPIT.BigData.Services
{
	internal class Node : INode
	{
		public string Name { get; set; }
		public string ConnectionString { get; set; }
		public string AdminConnectionString { get; set; }
		public Guid Token { get; set; }
		public NodeStatus Status { get; set; }
		public long Size { get; set; }
	}
}
