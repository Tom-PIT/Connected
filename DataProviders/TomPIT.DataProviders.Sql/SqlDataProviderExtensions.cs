using System.Collections.Generic;
using System.Linq;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql
{
	internal static class SqlDataProviderExtensions
	{
		public static ITable Find(this List<ITable> tables, string schema, string name)
		{
			return tables.FirstOrDefault(f => string.Compare(schema, f.Schema, true) == 0 && string.Compare(name, f.Name, true) == 0);
		}

		public static IView Find(this List<IView> views, string schema, string name)
		{
			return views.FirstOrDefault(f => string.Compare(schema, f.Schema, true) == 0 && string.Compare(name, f.Name, true) == 0);
		}

		public static ITableColumn FindColumn(this ITable table, string name)
		{
			return table.Columns.FirstOrDefault(f => string.Compare(name, f.Name, true) == 0);
		}

		public static IRoutine Find(this List<IRoutine> routines, string schema, string name)
		{
			return routines.FirstOrDefault(f => string.Compare(schema, f.Schema, true) == 0 && string.Compare(name, f.Name, true) == 0);
		}

		public static ITable FindPrimaryKeyTable(this IDatabase database, string name)
		{
			foreach (var i in database.Tables)
			{
				foreach (var j in i.Columns)
				{
					foreach (var k in j.Constraints)
					{
						if (string.Compare(k.Type, "PRIMARY KEY", true) == 0 && string.Compare(k.Name, name, true) == 0)
							return i;
					}
				}
			}

			return null;
		}

		public static ITableColumn FindPrimaryKeyColumn(this IDatabase database, string name)
		{
			foreach (var i in database.Tables)
			{
				foreach (var j in i.Columns)
				{
					foreach (var k in j.Constraints)
					{
						if (string.Compare(k.Type, "PRIMARY KEY", true) == 0 && string.Compare(k.Name, name, true) == 0)
							return j;
					}
				}
			}

			return null;
		}

		public static ITableColumn ResolvePrimaryKeyColumn(this ITable table)
		{
			foreach (var i in table.Columns)
			{
				foreach (var j in i.Constraints)
				{
					if (string.Compare(j.Type, "PRIMARY KEY", true) == 0)
						return i;
				}
			}

			return null;
		}

		public static ITableConstraint ResolvePrimaryKey(this ITable table)
		{
			foreach (var i in table.Columns)
			{
				foreach (var j in i.Constraints)
				{
					if (string.Compare(j.Type, "PRIMARY KEY", true) == 0)
						return j;
				}
			}

			return null;
		}

		public static List<ITableColumn> ResolveDefaults(this ITable table)
		{
			var r = new List<ITableColumn>();

			foreach (var i in table.Columns)
			{
				if (!string.IsNullOrWhiteSpace(i.DefaultValue))
					r.Add(i);
			}

			return r;
		}

		public static List<ITableConstraint> ResolveUniqueConstraints(this ITable table)
		{
			var r = new List<ITableConstraint>();

			foreach (var i in table.Columns)
			{
				foreach (var j in i.Constraints)
				{
					if (string.Compare(j.Type, "UNIQUE", true) == 0)
						r.Add(j);
				}
			}

			return r;
		}

		public static ITableColumn FindUniqueConstraintColumn(this IDatabase database, string name)
		{
			var r = new List<ITableConstraint>();

			foreach (var i in database.Tables)
			{
				foreach (var j in i.Columns)
				{
					foreach (var k in j.Constraints)
					{
						if (string.Compare(k.Type, "UNIQUE", true) == 0 && string.Compare(k.Name, name, true) == 0)
							return j;
					}
				}
			}

			return null;
		}


	}
}
