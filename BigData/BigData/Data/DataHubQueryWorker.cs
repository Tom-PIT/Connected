using Amt.Sdk.DataHub;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amt.DataHub.Data
{
	internal class DataHubQueryWorker
	{
		private DataHubQueryContext _ctx = null;

		public void Execute(DataHubQueryContext ctx, DataHubCommandTextParser parser, ConcurrentQueue<PartitionFile> queue, List<Tuple<string, object>> parameters, DataTable schema)
		{
			_ctx = ctx;

			Result = schema.Clone();

			while (!queue.IsEmpty)
			{
				PartitionFile file = null;

				if (queue.TryDequeue(out file))
					ExecuteSelect(parser, file, parameters);

				if (_ctx.IsFull)
					return;
			}
		}

		private void ExecuteSelect(DataHubCommandTextParser parser, PartitionFile file, List<Tuple<string, object>> parameters)
		{
			var node = AmtShell.GetService<INodeService>().Select(file.NodeId);

			var r = new NodeReader<DataHubQueryRecord>(node, ParseCommandText(parser, file), CommandType.Text);

			if (parameters != null)
			{
				foreach (var i in parameters)
				{
					//if (parser.Where.Contains(i.Item1))
					r.CreateParameter(i.Item1, i.Item2);
				}
			}

			r.Execute();

			int count = r.Result.Count;

			_ctx.IncrementCount(count);

			if (_ctx.IsFull)
				return;

			foreach (var i in r.Result)
				Result.Rows.Add(i.ItemArray);
		}

		public DataTable Result { get; private set; }

		private string ParseCommandText(DataHubCommandTextParser parser, PartitionFile file)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("SELECT {0} ", parser.Select);
			sb.AppendFormat(" FROM [{0}]", string.Format("t_{0}", file.FileId.AsString()));

			if (!string.IsNullOrWhiteSpace(parser.Where))
				sb.AppendFormat(" WHERE {0}", parser.Where);

			return sb.ToString();
		}
	}
}