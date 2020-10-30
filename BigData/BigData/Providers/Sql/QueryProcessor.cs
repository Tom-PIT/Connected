using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Transactions;

namespace TomPIT.BigData.Providers.Sql
{
	internal class QueryProcessor
	{
		private List<IPartitionFile> _files = null;
		public QueryProcessor(Query owner)
		{
			Owner = owner;
		}

		private Query Owner { get; }
		public List<IPartitionFile> Files
		{
			get
			{
				if (_files == null)
					_files = new List<IPartitionFile>();

				return _files;
			}
		}

		public JArray Execute()
		{
			var result = new JArray();

			foreach (var file in Files)
			{
				var r = Execute(file);

				if (r != null)
				{
					foreach (var item in r)
						result.Add(item);
				}
			}

			return result;
		}

		private JArray Execute(IPartitionFile file)
		{
			var node = Owner.Context.Tenant.GetService<INodeService>().Select(file.Node);
			var reader = new NodeReader<QueryRecord>(node, ParseCommandText(file), System.Data.CommandType.Text);

			foreach (var parameter in Owner.Parameters)
			{
				if (parameter.Name.StartsWith("@"))
					reader.CreateParameter(parameter.Name, parameter.Value);
			}

			var records = reader.Execute();
			var result = new JArray();

			foreach (var record in records)
			{
				var jo = new JObject();

				foreach (var item in record.ItemArray)
				{
					if (string.Compare(item.Name, Merger.IdColumn, true) == 0)
						continue;

					jo.Add(item.Name, new JValue(item.Value));
				}

				result.Add(jo);
			}

			return result;
		}

		private string ParseCommandText(IPartitionFile file)
		{
			var text = Owner.CommandText.Replace("@file", $"t_{file.FileName.ToString().Replace("-", string.Empty)}");

			return text;
		}
	}
}
