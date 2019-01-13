using Amt.Data;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Data
{
	internal class NodeAdminScalarReader<T> : ScalarReader<T>
	{
		private INode _node = null;
		public NodeAdminScalarReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
		{
			_node = node;
		}

		protected override string ConnectionString
		{
			get
			{
				return string.IsNullOrWhiteSpace(_node.AdminConnectionString)
					 ? _node.ConnectionString
					 : _node.AdminConnectionString;
			}
		}
	}
}
