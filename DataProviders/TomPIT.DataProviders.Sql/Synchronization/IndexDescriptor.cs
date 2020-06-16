using System.Collections.Generic;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class IndexDescriptor
	{
		private List<string> _columns = null;

		public bool Unique { get; set; }
		public string Group { get; set; }

		public List<string> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<string>();

				return _columns;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Group) ? Columns[0] : Group;
		}
	}
}
