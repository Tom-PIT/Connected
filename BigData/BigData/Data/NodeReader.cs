using Amt.Api.Data;
using Amt.Sdk.DataHub;
using Amt.Sys.Model;
using System.Data;

namespace Amt.DataHub.Data
{
	internal class NodeReader<T> : Reader<T> where T : Record, new()
	{
		private INode _node = null;
		public NodeReader(INode node, string commandText, CommandType commandType) : base(commandText, commandType)
		{
			_node = node;
		}

		protected override string ConnectionString
		{
			get { return _node.ConnectionString; }
		}
	}
}