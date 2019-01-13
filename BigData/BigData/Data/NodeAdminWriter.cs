using Amt.Data;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Data
{
	internal class NodeAdminWriter : Writer
	{
		private INode _node = null;
		public NodeAdminWriter(INode node, string commandText) : base(commandText)
		{
			_node = node;
		}

		public NodeAdminWriter(INode node, string commandText, CommandType type) : base(commandText, type)
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
