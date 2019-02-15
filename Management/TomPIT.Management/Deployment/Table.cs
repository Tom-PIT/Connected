using Newtonsoft.Json;
using System.Collections.Generic;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment
{
	public class Table : SchemaBase, ITable
	{
		private List<ITableIndex> _indexes = null;
		[JsonProperty(PropertyName = "columns")]
		public List<ITableColumn> Columns { get; set; }
		[JsonProperty(PropertyName = "indexes")]
		public List<ITableIndex> Indexes
		{
			get
			{
				if (_indexes == null)
					_indexes = new List<ITableIndex>();

				return _indexes;
			}
		}
	}
}
