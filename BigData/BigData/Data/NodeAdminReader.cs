using Amt.Data;
using Amt.Data.Common;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Data
{
	internal class NodeAdminReader<T> : Reader<T> where T : DatabaseRecord, new()
	{
		private INode _node = null;
		public NodeAdminReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
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