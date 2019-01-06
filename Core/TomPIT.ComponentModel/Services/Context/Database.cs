using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	/// <summary>
	/// Base class for DatabaseGet and DatabasePost.
	/// </summary>
	internal class Database : ContextClient
	{
		public Database(IExecutionContext context) : base(context)
		{
		}
		/// <summary>
		/// This method creates connection for the sepcified id.
		/// </summary>
		/// <remarks>
		/// Clients share the same instance thus they should never make any changes on returned instance.
		/// </remarks>
		/// <param name="sender">Current execution descriptor.</param>
		/// <param name="id">The connection id.</param>
		/// <param name="authorityName">The authority name for which connection is requested. This argument serves for
		/// exception handling and diagnostic purposes.</param>
		/// <returns>IConnection instance if a valid one is found.</returns>
		protected IConnection CreateConnection(IExecutionContext context, IExecutionContextState sender, Guid id, string authorityName)
		{
			/*
			 * The client has not set a connection.
			 */
			if (id == Guid.Empty)
				throw ExecutionException.ConnectionNotSet(context, CreateDescriptor(ExecutionEvents.CreateConnection, sender.Authority, sender.Id, sender.Property, sender.MicroService), authorityName);
			/*
			 * The client has invalid connection set. This is probably due to the deleted connection or misbehaved import.
			 */
			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(id) is IConnection connection))
				throw ExecutionException.ConnectionNotFound(context, CreateDescriptor(ExecutionEvents.CreateConnection, sender.Authority, sender.Id, sender.Property, sender.MicroService), authorityName);
			/*
			 * We don't allow disabled connections to execute
			 */
			if (!connection.Enabled)
				throw ExecutionException.Create(context, string.Format("{0} ({1}, {2}).", SR.ErrConnectionDisabled, authorityName, connection.ComponentName(context)), CreateDescriptor(ExecutionEvents.CreateConnection, sender.Authority, sender.Id, sender.Property, sender.MicroService));

			return connection;
		}
		/// <summary>
		/// This method creates data provider for the valid connection.
		/// </summary>
		/// <param name="sender">Current execution descriptor</param>
		/// <param name="connection">A connection instance which holds the information about its data provider</param>
		/// <returns>IDataProvider instance is a valid one is found.</returns>
		protected IDataProvider CreateDataProvider(IExecutionContext context, IExecutionContextState sender, IConnection connection)
		{
			/*
			 * Connection is not properly configured. We just notify the user about the issue.
			 */
			if (connection.DataProvider == Guid.Empty)
				throw ExecutionException.ConnectionDataProviderNotSet(context, CreateDescriptor(sender.Event, ExecutionContextState.Connection, connection.Component.ToString(), null, sender.MicroService), connection.ComponentName(context));

			var provider = context.Connection().GetService<IDataProviderService>().Select(connection.DataProvider);
			/*
			 * Connection has invalid data provider set. This can be for various reasons:
			 * --------------------------------------------------------------------------
			 * - data provider has been removed from the configuration
			 * - package misbehavior
			 */
			if (provider == null)
				throw ExecutionException.ConnectionDataProviderNotFound(Context, CreateDescriptor(sender.Event, ExecutionContextState.Connection, connection.Component.ToString(), null, sender.MicroService), connection.ComponentName(Context));

			return provider;
		}
		/// <summary>
		/// Different data providers enforces different naming rules. The infrastructure is trying
		/// to be smart and offers help so the clients can pass parameter names without @s and $s.
		/// </summary>
		/// <param name="name">The passed parameter name</param>
		/// <returns>List of a parameter names that can be used as a valid qualifiers for the specified parameter.</returns>
		protected List<string> ParameterQualifiers(string name)
		{
			/*
			 * first we remove some characters in order to get as native qualifier
			 * as possible
			 */
			var raw = string.Empty;
			/*
			 * SQL server
			 */
			if (name.StartsWith("@"))
				raw = name.Substring(1);
			/*
			 * it's common to use $ as a variable or parameter name
			 */
			else if (name.StartsWith("$"))
				raw = name.Substring(1);
			else
				raw = name;
			/*
			 * Now let's add different qualifier versions along with raw one
			 */
			return new List<string>()
			{
				raw,
				string.Format("@{0}", raw),
				string.Format("${0}", raw)
			};
		}
		/// <summary>
		/// This method verifies if the specified argument is defined on the data source.
		/// </summary>
		/// <param name="sender">Current execution descriptor</param>
		/// <param name="name">The name of the argument.</param>
		/// <param name="parameters">The data source parameters.</param>
		/// <returns>true if the parameter is defined on the data source, false otherwise.</returns>
		protected bool IsDefined(IExecutionContextState sender, string name, List<IDataParameter> parameters)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw ExecutionException.ParameterNameNotSet(Context, CreateDescriptor(0));
			/*
			 * We'll try with different name combinations as defined in ParameterQualifiers method
			 */
			foreach (var i in parameters)
			{
				foreach (var j in ParameterQualifiers(name))
				{
					if (string.Compare(i.Name, j, true) == 0)
						/*
						 * Found
						 */
						return true;
				}
			}
			/*
			 * Not passed
			 */
			return false;
		}
		/// <summary>
		/// This method looks in the passed parameters collection and tries to find the matched parameter
		/// by its name. It tries with different qualifiers
		/// </summary>
		/// <param name="sender">Current execution descriptor.</param>
		/// <param name="name">The name of the parameter to search for.</param>
		/// <param name="arguments">List od passed arguments.</param>
		/// <param name="value">Returns passed value if argument is found.</param>
		/// <returns>true if the argument's value is found and set, false otherwise.</returns>
		protected bool MatchArgument(IExecutionContextState sender, string name, JObject arguments, out object value)
		{
			value = null;

			if (string.IsNullOrWhiteSpace(name))
				throw ExecutionException.ParameterNameNotSet(Context, sender);
			/*
			 * It is not necessary the client is passing parameters at all. In that case
			 * we simply return false.
			 */
			if (arguments == null)
				return false;

			foreach (var i in ParameterQualifiers(name))
			{
				/*
				 * case insensitive search because we can't rely on the client
				 * to always pass StringComparer.InvariantCultureIgnoreCase
				 * to dictionary constructor
				 */
				foreach (var j in arguments)
				{
					if (string.Compare(j.Key.ToString(), i, true) == 0)
					{
						/*
						 * Name matched. Let's return its value.
						 */
						value = ((JValue)arguments[i]).Value;
						return true;
					}
				}
			}
			/*
			 * Parameter has not been passed.
			 */
			return false;
		}
		/// <summary>
		/// This method processes parameter value based on its configuration.
		/// </summary>
		/// <param name="sender">Current execution descriptor.</param>
		/// <param name="parameter">Parameter configuration.</param>
		/// <param name="proposedValue">Proposed value. It can be from the client or by handling the Prepared event.</param>
		/// <param name="value">The actual value the should be used by a command.</param>
		/// <param name="direction">The direction of the command parameter</param>
		/// <returns>true if the parameter value has been successfully processed, false otherwise.</returns>
		protected bool Value(IExecutionContextState sender, IDataParameter parameter, object proposedValue, out object value, out System.Data.ParameterDirection direction)
		{
			/*
			 * Fot the return value parameters the valu should not be set regardless of the proposed value
			 * because its value is set as a result of the execution.
			 */
			if (parameter is IReturnValueParameter)
			{
				direction = System.Data.ParameterDirection.ReturnValue;
				value = null;

				return true;
			}
			/*
			 * parameter value can come from different sources and can behave in different ways.
			 * First, we need to resolve its value and then we'll resolve its behavior.
			 */
			direction = System.Data.ParameterDirection.Input;
			value = proposedValue;

			if (parameter.NullMapping)
				value = MapNull(parameter.DataType, value);
			/*
			 * if the parameter value is null there are two scenarios:
			 * - parameter is nullable; in that case we'll allow the process to continue its execution
			 * - parameter is not nullable; in that case an exception will be thrown
			 */
			if (proposedValue == null || value == DBNull.Value)
			{
				if (!parameter.IsNullable)
					throw ExecutionException.ParameterExpected(Context, sender, parameter.Name);
				/*
				 * in any case we'll stop the processing of the parameter on that point
				 * because it contains no value
				 */
				return false;
			}
			/*
			 * Find out the underlying system type for the parameter configuration so we'll
			 * be able to convert the value to the destination type.
			 */
			var type = Types.ToType(parameter.DataType);
			/*
			 * We we'll try to convert that value to the coresponding type. If the conversion
			 * won't be successfull an exception will be thrown.
			 */
			if (!Types.TryConvert(value, out object converted, type))
				throw ExecutionException.ParameterConversion(Context, sender, parameter.Name, value.ToString(), type.Name);
			/*
			 * Now that's the value has been resolved we need to process its behavior.
			 */
			value = ProcessTimezone(sender, parameter, proposedValue);

			return true;
		}

		private object MapNull(DataType type, object value)
		{
			if (value == null || value == DBNull.Value)
				return value;

			switch (type)
			{
				case DataType.String:
					var sv = Types.Convert<string>(value);

					if (string.IsNullOrWhiteSpace(sv))
						return DBNull.Value;
					break;
				case DataType.Integer:
				case DataType.Float:
				case DataType.Long:
					var dv = Types.Convert<decimal>(value);

					if (dv == decimal.Zero)
						return DBNull.Value;

					break;
				case DataType.Date:
					var dtv = Types.Convert<DateTime>(value);

					if (dtv == DateTime.MinValue)
						return DBNull.Value;

					break;
				case DataType.Guid:
					var gv = Types.Convert<Guid>(value);

					if (gv == Guid.Empty)
						return DBNull.Value;
					break;
				case DataType.Binary:
					var bv = Types.Convert<byte[]>(value);

					if (bv == null || bv.Length == 0)
						return DBNull.Value;

					break;
			}

			return value;
		}

		/// <summary>
		/// This method check the configuration for automatic Timezone conversion. If parameter supports
		/// conversion the value will be automatically converted to UTC date based on the current identity.
		/// </summary>
		/// <param name="sender">Current execution descriptor.</param>
		/// <param name="parameter">The parameter configuration</param>
		/// <param name="proposedValue">The original (unconverted) value.</param>
		/// <returns>rocessed value regardless if it has been converted or not.</returns>
		private object ProcessTimezone(IExecutionContextState sender, IDataParameter parameter, object proposedValue)
		{
			/*
			 * Not set. return proposedValue.
			 */
			if (!parameter.SupportsTimeZone)
				return proposedValue;
			/*
			 * Conversion is supported only on parameters with DataType set to date.
			 * We'll throw exception here because it's beter the client to know the configuration
			 * is incorrent that to try to find out why the value has not been converted.
			 */
			if (parameter.DataType != DataType.Date)
				throw ExecutionException.TimezoneParametersSupportedOnDatesOnly(Context, sender, parameter.Name);
			/*
			 * The date value will be converter to the utc.
			 */
			return Context.Services.Timezone.ToUtc((DateTime)proposedValue);
		}
		/// <summary>
		/// This method checks for a valid parameter values based on the configuration data type.
		/// </summary>
		/// <param name="sender">Current execution descriptor.</param>
		/// <param name="parameter">The parameter configuration</param>
		/// <param name="value">The parameter value</param>
		protected void ValidateParameterDatatype(IExecutionContextState sender, IDataParameter parameter, object value)
		{
			/*
			 * If the value is not defined thats not the case of
			 * this method. We simply return, because null validation is handled
			 * elsewhere.
			 */
			if (!Types.IsValueDefined(value))
				return;

			switch (parameter.DataType)
			{
				case DataType.String:
					if (value is string || value is char)
						return;

					break;
				case DataType.Integer:
					if (value is int || value is uint
						&& value is byte || value is sbyte
						&& value is short || value is ushort
						&& value is long || value is ulong)
						return;

					break;
				case DataType.Float:
					if (value is float
						&& value is double
						&& value is decimal)
						return;

					break;
				case DataType.Date:
					if (value is DateTime)
						return;

					break;
				case DataType.Bool:
					if (value is bool)
						return;

					break;
				case DataType.Guid:
					if (value is Guid)
						return;

					break;
				case DataType.Binary:
					if (value is byte[])
						return;

					break;
				case DataType.Long:
					if (value is long)
						return;
					break;
				default:
					throw new NotSupportedException();
			}

			throw ExecutionException.InvalidParameterDataType(Context, sender, parameter.Name, value.ToString(), parameter.DataType.ToString());
		}
		/// <summary>
		/// This method verifies that passed arguments are actually defined on the data source.
		/// </summary>
		/// <remarks>
		/// There is one exception to this routine. If the passed arguments are RequestArguments there is not checking because
		/// RequestArguments holds all parameters from the HttpRequest, not only those required for the data source call. Tha way,
		/// it is impossible to know from request which parameters are meant for data source and which not.
		/// </remarks>
		/// <param name="sender">The current execution descriptor.</param>
		/// <param name="parameters">Data source parameters</param>
		/// <param name="arguments">Passed arguments</param>
		protected void ValidateArguments(IExecutionContextState sender, List<IDataParameter> parameters, JObject arguments)
		{
			if (arguments == null)
				return;

			foreach (var i in arguments)
			{
				if (!IsDefined(sender, i.Key, parameters))
					throw ExecutionException.ParameterNotDefined(Context, sender, i.Key);
			}
		}

		protected void SetCommandParameters(IExecutionContextState sender, IDataCommandDescriptor command, List<IDataParameter> configuration, JObject arguments)
		{
			var pars = new ListItems<IDataParameter>();

			foreach (var i in configuration)
			{
				if (i is IReturnValueParameter)
				{
					var cp = new CommandParameter()
					{
						DataType = Types.ToType(i.DataType),
						Direction = System.Data.ParameterDirection.ReturnValue,
						Name = i.Name
					};

					command.Parameters.Add(cp);

					continue;
				}

				if (MatchArgument(sender, i.Name, arguments, out object value)
					&& Value(sender, i, value, out object actualValue, out System.Data.ParameterDirection direction))
				{
					var cp = new CommandParameter()
					{
						DataType = Types.ToType(i.DataType),
						Direction = direction,
						Name = i.Name,
						Value = actualValue
					};

					command.Parameters.Add(cp);
				}
				else
				{
					if (!i.IsNullable)
						throw ExecutionException.ParameterValueNotSet(Context, sender, i.Name);
				}
			}
		}
	}
}