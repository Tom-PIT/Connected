using System.Collections.Generic;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class Table : SchemaBase, ITable
	{
		private List<ITableColumn> _columns = null;
		private List<ITableIndex> _indexes = null;

		public List<ITableColumn> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<ITableColumn>();

				return _columns;
			}
		}

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
