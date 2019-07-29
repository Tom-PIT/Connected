using System.Data;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class NodeAdminReader<T> : Reader<T> where T : DatabaseRecord, new()
	{
		public NodeAdminReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
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