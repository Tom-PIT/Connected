﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.ComponentModel.Apis
{
	internal abstract class DataCommand : IDataCommand
	{
		private List<TomPIT.Data.IDataParameter> _parameters = null;

		public string CommandText { get; set; }
		public CommandType CommandType { get; set; } = CommandType.StoredProcedure;
		public int CommandTimeout { get; set; } = 30;
		public IDataConnection Connection { get; set; }

		protected DataCommand(IExecutionContext context)
		{
			Context = context;
		}

		protected IExecutionContext Context { get; }

		public List<TomPIT.Data.IDataParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<TomPIT.Data.IDataParameter>();

				return _parameters;
			}
		}

		public TomPIT.Data.IDataParameter SetParameter(string name, object value)
		{
			return SetParameter(name, value, false);
		}

		public TomPIT.Data.IDataParameter SetParameter(string name, object value, bool nullMapping)
		{
			var parameter = Parameters.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
			var mappedValue = nullMapping ? MapNullValue(value) : value;

			if (parameter == null)
			{
				parameter = new DataParameter
				{
					Name = name,
					Value = mappedValue
				};

				Parameters.Add(parameter);
			}

			parameter.Value = mappedValue;

			return parameter;
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

		private string TrimValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return value.Trim();
		}

		protected IDataCommandDescriptor CreateCommand()
		{
			var r = new DataCommandDescriptor
			{
				CommandText = CommandText,
				CommandType = CommandType,
				CommandTimeout = CommandTimeout
			};

			foreach(var parameter in Parameters)
			{
				r.Parameters.Add(new CommandParameter
				{
					Direction = parameter.Direction,
					Name = parameter.Name,
					Value = parameter.Value
				});
			}

			return r;
		}

		public bool CloseConnection { get; set; }
	}
}