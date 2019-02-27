using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public abstract class DatabaseCommand : DatabaseConnection
	{
		private List<SqlParameter> _parameters = null;

		protected DatabaseCommand()
		{

		}

		protected DatabaseCommand(string commandText)
		{
			CommandText = commandText;
		}

		protected string CommandText { get; set; }

		protected List<SqlParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<SqlParameter>();

				return _parameters;
			}
		}

		public void ModifyParameter(string parameterName, object value)
		{
			ModifyParameter(parameterName, value, false);
		}

		public void ModifyParameter(string parameterName, object value, bool mapNull)
		{
			SqlParameter ex = null;

			foreach (var p in Parameters)
			{
				if (string.Compare(p.ParameterName, parameterName, false) == 0)
				{
					ex = p;
					break;
				}
			}

			if (ex != null)
			{
				if (mapNull)
					ex.Value = MapNullValue(value);
				else
				{
					if (value is string)
						ex.Value = TrimValue(value as string);
					else
						ex.Value = value;
				}
			}
		}

		private object MapNullValue(object value)
		{
			if (value == null || value == DBNull.Value)
				return DBNull.Value;

			else if (value is string)
			{
				string v = TrimValue(value as string);

				if (string.IsNullOrWhiteSpace(v))
					return DBNull.Value;
			}
			else if (value is DateTime)
			{
				if ((DateTime)value == DateTime.MinValue)
					return DBNull.Value;
			}
			else if (value is int || value is float || value is double || value is short || value is byte || value is long || value is decimal)
			{
				if (Convert.ToDecimal(value) == decimal.Zero)
					return DBNull.Value;
			}
			else if (value is byte[])
			{
				if (value == null || ((byte[])value).Length == 0)
					return DBNull.Value;
			}
			else if (value is Guid)
			{
				if ((Guid)value == Guid.Empty)
					return DBNull.Value;
			}
			else if (value is Enum)
			{
				if ((int)value == 0)
					return DBNull.Value;
			}
			else if (value is TimeSpan)
			{
				if ((TimeSpan)value == TimeSpan.Zero)
					return DBNull.Value;
			}

			return value;
		}

		public SqlParameter CreateParameter(string parameterName, object value)
		{
			var p = new SqlParameter
			{
				ParameterName = parameterName
			};

			if (value is string)
				p.Value = TrimValue(value as string);
			else if (value is DataTable)
			{
				p.SqlDbType = SqlDbType.Structured;
				p.Value = value;
			}
			else if (value is DateTime)
			{
				p.SqlDbType = SqlDbType.DateTime2;
				p.Value = value;
			}
			else if (value is JObject || value is JArray)
			{
				p.SqlDbType = SqlDbType.NVarChar;
				p.Value = JsonConvert.SerializeObject(value);
			}
			else
				p.Value = value;

			Parameters.Add(p);

			return p;
		}

		public void CreateParameter(string parameterName, object value, bool nullMapping)
		{
			if (nullMapping)
			{
				object v = MapNullValue(value);

				if (v == DBNull.Value)
					return;
			}

			CreateParameter(parameterName, value);
		}

		private string TrimValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return value.Trim();
		}
	}
}