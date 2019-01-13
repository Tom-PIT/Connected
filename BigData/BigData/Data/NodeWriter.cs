using Amt.Data;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Data
{
	internal class NodeWriter : Writer
	{
		private INode _node = null;
		public NodeWriter(INode node, string commandText) : base(commandText)
		{
			_node = node;
		}

		public NodeWriter(INode node, string commandText, CommandType type) : base(commandText, type)
		{
			_node = node;
		}

		protected override string ConnectionString { get { return _node.ConnectionString; } }
	}
}
