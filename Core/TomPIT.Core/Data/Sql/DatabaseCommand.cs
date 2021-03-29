using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

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
					ex.Value = NullMapper.Map(value, MappingMode.Database);
				else
				{
					if (value is string)
						ex.Value = NullMapper.Trim(value as string);
					else
						ex.Value = value;
				}
			}
		}


		public SqlParameter CreateParameter(string parameterName, object value)
		{
			var p = new SqlParameter
			{
				ParameterName = parameterName
			};

			if (value is string)
				p.Value = NullMapper.Trim(value as string);
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
				p.Value = Serializer.Serialize(value);
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
				object v = NullMapper.Map(value, MappingMode.Database);

				if (v == DBNull.Value)
					return;
			}

			CreateParameter(parameterName, value);
		}
	}
}