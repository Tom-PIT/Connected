using System.Data;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class NodeAdminWriter : Writer
	{
		public NodeAdminWriter(INode node, string commandText) : base(commandText)
		{
			Node = node;
		}

		public NodeAdminWriter(INode node, string commandText, CommandType type) : base(commandText, type)
		{
			Node = node;
		}

		private INode Node { get; }
		protected override string ConnectionString
		{
			get
			{
				return string.IsNullOrWhiteSpace(Node.AdminConnectionString)
					 ? Node.ConnectionString
					 : Node.AdminConnectionString;
			}
		}
	}
}
