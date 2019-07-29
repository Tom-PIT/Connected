using System.Data;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class NodeReader<T> : Reader<T> where T : DatabaseRecord, new()
	{
		public NodeReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
		{
			Node = node;
		}

		private INode Node = null;

		protected override string ConnectionString
		{
			get { return Node.ConnectionString; }
		}
	}
}