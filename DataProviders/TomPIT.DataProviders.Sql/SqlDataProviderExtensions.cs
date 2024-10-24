﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Data;
using TomPIT.DataProviders.Sql.Synchronization;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql
{
	internal static class SqlDataProviderExtensions
	{
		#region Deployment
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
					if (string.Compare(j.Type, "UNIQUE", true) == 0 && r.FirstOrDefault(f => string.Compare(f.Name, j.Name, false) == 0) == null)
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
		#endregion

		public static T GetValue<T>(this IDataReader r, string fieldName, T defaultValue)
		{
			var idx = r.GetOrdinal(fieldName);

			if (idx == -1)
				return defaultValue;

			if (r.IsDBNull(idx))
				return defaultValue;

			return (T)Convert.ChangeType(r.GetValue(idx), typeof(T));
		}

		public static string SchemaName(this IModelSchema model)
		{
			return string.IsNullOrWhiteSpace(model.Schema) ? "dbo" : model.Schema;
		}

		public static string FileGroup(this ISynchronizer synchronizer)
		{
			return "PRIMARY";
		}

		public static string ParseDefaultValue(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			if (value.StartsWith("N'"))
				return value;

			var defValue = $"N'{value}'";

			if (value.Length > 1)
			{
				var last = value.Trim()[^1];
				var prev = value.Trim()[0..^1].Trim()[^1];

				if (last == ')' && prev == '(')
					defValue = value;
			}

			return defValue;
		}
	}
}
