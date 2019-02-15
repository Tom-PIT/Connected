using System.Collections.Generic;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class TableIndex : ITableIndex
	{
		private List<string> _columns = null;

		public string Name { get; set; }

		public List<string> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<string>();

				return _columns;
			}
		}
	}
}
