using System.Data;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class NodeAdminScalarReader<T> : ScalarReader<T>
	{
		public NodeAdminScalarReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
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
