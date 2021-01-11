using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	internal abstract class DataCommand : MiddlewareObject, IDataCommand
	{
		private object _sync = new object();
		private List<TomPIT.Data.IDataParameter> _parameters = null;

		public string CommandText { get; set; }
		public CommandType CommandType { get; set; } = CommandType.StoredProcedure;
		public int CommandTimeout { get; set; } = 30;
		public IDataConnection Connection { get; set; }

		protected IDataCommandDescriptor Command { get; private set; }

		protected DataCommand(IMiddlewareContext context) : base(context)
		{
		}

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
			return ResolveParameter(name, value, nullMapping, null);
		}
		public TomPIT.Data.IDataParameter SetParameter(string name, object value, bool nullMapping, DbType type)
		{
			return ResolveParameter(name, value, nullMapping, type);
		}

		private TomPIT.Data.IDataParameter ResolveParameter(string name, object value, bool nullMapping, DbType? type)
		{
			var parameter = Parameters.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
			var mappedValue = nullMapping ? MapNullValue(value) : MapValue(value);
			var dbType = type != null ? (DbType)type : value == null || value == DBNull.Value ? DbType.String : Types.ToDbType(value.GetType());

			if (parameter == null)
			{
				parameter = new DataParameter
				{
					Name = name,
					Value = mappedValue,
					Type = dbType
				};

				Parameters.Add(parameter);
			}

			parameter.Value = mappedValue;

			return parameter;
		}

		private Type ResolveType(object value)
		{
			if (value == null || value == DBNull.Value)
				return typeof(string);

			return value.GetType();
		}

		private object MapValue(object value)
		{
			if (value == null || value == DBNull.Value)
				return value;

			if (value.GetType().IsTypePrimitive())
			{
				if (value.GetType().IsEnum)
					return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

				return value;
			}
			else if (value is byte[])
				return value;

			return Serializer.Serialize(value);
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
			else
				return Serializer.Serialize(value);

			return value;
		}

		private string TrimValue(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return value.Trim();
		}

		protected void EnsureCommand()
		{
			if (Command == null)
			{
				lock (_sync)
				{
					if (Command == null)
						Command = CreateCommand();
				}
			}

			SynchronizeParameters();
		}

		private void SynchronizeParameters()
		{
			Command.Parameters.Clear();

			foreach (var parameter in Parameters)
			{
				Command.Parameters.Add(new CommandParameter
				{
					Direction = parameter.Direction,
					Name = parameter.Name,
					Value = parameter.Value,
					DataType = parameter.Type
				});
			}
		}

		private IDataCommandDescriptor CreateCommand()
		{
			var r = new DataCommandDescriptor
			{
				CommandText = CommandText,
				CommandType = CommandType,
				CommandTimeout = CommandTimeout
			};

			return r;
		}

		#region IDisposable

		public void Dispose()
		{
			if (_parameters != null)
			{
				_parameters.Clear();
				_parameters = null;
			}

			CommandText = null;

			Connection = null;

			OnDispose();
		}

		protected virtual void OnDispose()
		{
		}

		#endregion
	}
}
