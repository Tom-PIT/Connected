using System;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	public enum IndexType
	{
		Index = 1,
		Unique = 2,
		PrimaryKey = 3
	}
	internal class ObjectIndex
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Keys { get; set; }

		public IndexType Type
		{
			get
			{
				var tokens = Description.Split(',');
				var result = IndexType.Index;

				foreach (var token in tokens)
				{
					if (token.Trim().Contains("primary key", StringComparison.OrdinalIgnoreCase))
						return IndexType.PrimaryKey;
					else if (string.Compare(token.Trim(), "unique", true) == 0)
						result = IndexType.Unique;
				}

				return result;
			}
		}

		public bool IsReferencedBy(string column)
		{
			return Columns.Contains(column, StringComparer.OrdinalIgnoreCase);
		}

		public List<string> Columns
		{
			get
			{
				var result = new List<string>();
				var tokens = Keys.Split(',');

				foreach (var token in tokens)
					result.Add(token.Trim());

				return result;
			}
		}
	}
}
