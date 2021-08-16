using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Data.DataProviders;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Data
{
    internal abstract class DataCommand : MiddlewareObject, IDataCommand
    {
        private object _sync = new object();
        private List<IDataParameter> _parameters = null;

        public string CommandText { get; set; }
        public CommandType CommandType { get; set; } = CommandType.StoredProcedure;
        public int CommandTimeout { get; set; } = 30;
        public IDataConnection Connection { get; set; }

        protected IDataCommandDescriptor Command { get; private set; }

        protected DataCommand(IMiddlewareContext context) : base(context)
        {
        }

        public List<IDataParameter> Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new List<IDataParameter>();

                return _parameters;
            }
        }

        public IDataParameter SetParameter(string name, object value)
        {
            return SetParameter(name, value, false);
        }

        public IDataParameter SetParameter(string name, object value, bool nullMapping)
        {
            return ResolveParameter(name, value, nullMapping, null);
        }
        public IDataParameter SetParameter(string name, object value, bool nullMapping, DbType type)
        {
            return ResolveParameter(name, value, nullMapping, type);
        }

        private IDataParameter ResolveParameter(string name, object value, bool nullMapping, DbType? type)
        {
            var parameter = Parameters.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
            var mappedValue = nullMapping ? NullMapper.Map(value, MappingMode.Database) : MapValue(value);

            var rawValue = value is INullableProperty np ? np.MappedValue : value;
            var dbType = type != null ? (DbType)type : rawValue == null || rawValue == DBNull.Value ? DbType.String : Types.ToDbType(rawValue.GetType());

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

        private object MapValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return value;

            if (value.GetType().IsTypePrimitive())
            {
                if (value.GetType().IsEnum)
                    return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                else if (value is DateTimeOffset date)
                    return date.UtcDateTime;

                return value;
            }
            else if (value is byte[])
                return value;

            return Serializer.Serialize(value);
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
                if (parameter.Direction == ParameterDirection.Input && parameter.Value is null || parameter.Value == DBNull.Value)
                    continue;

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
        protected override void OnDisposing()
        {
            if (_parameters != null)
            {
                _parameters.Clear();
                _parameters = null;
            }

            CommandText = null;
            Connection = null;

            base.OnDisposing();
        }

        #endregion
    }
}
