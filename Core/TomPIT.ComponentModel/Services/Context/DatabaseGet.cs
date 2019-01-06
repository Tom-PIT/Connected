using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	internal class DatabaseGet : Database
	{

		public DatabaseGet(IExecutionContext context) : base(context)
		{

		}

		public JObject Execute(Guid microService, string name, JObject arguments)
		{
			var ctx = new ExecutionContext(Context);

			ctx.Identity.SetContextId(microService.ToString());

			var component = ctx.Connection().GetService<IComponentService>().SelectComponent(microService, "DataSource", name);

			if (component == null)
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name), CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.DatabaseGet, null, null, microService));

			if (!(ctx.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IDataSource config))
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name), CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.DatabaseGet, null, null, microService));

			var schema = CreateSchema(component, config);

			var d = CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.DatabaseGet, component.Token.ToString(), null, microService);

			var connection = CreateConnection(ctx, d, config.Connection, component.Name);
			var dataProvider = CreateDataProvider(ctx, d, connection);

			var preparing = ctx.Connection().Execute(config.Preparing, this, new PreparingArguments(ctx, arguments, schema));

			if (preparing.Cancel)
				return new JObject();

			var command = CreateCommand(d, config, connection, preparing.Arguments);

			var validating = ctx.Connection().Execute(config.Validating, this, new ValidatingArguments(ctx, command));

			if (validating.ValidationErrors.Count > 0)
				throw new ValidationException(validating.ValidationErrors);

			var executing = ctx.Connection().Execute(config.Executing, this, new ExecutingArguments(ctx, command, preparing.Schema));

			if (executing.Cancel)
				return new JObject();

			var result = dataProvider.Query(command, executing.Schema);

			var returnValues = new JObject();

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
					returnValues.Add(i.Name, JToken.FromObject(i.Value));
			}

			var executed = ctx.Connection().Execute(config.Executed, this, new ExecutedArguments(ctx, result, returnValues));

			return executed.Data;
		}

		private DataTable CreateSchema(IComponent component, IDataSource dataSource)
		{
			var r = new DataTable(component.Name);

			foreach (var i in dataSource.Fields)
				r.Columns.Add(CreateColumn(i));

			return r;
		}

		private DataColumn CreateColumn(IDataField field)
		{
			var r = new DataColumn
			{
				ColumnName = field.Name,
				DataType = Types.ToType(field.DataType)
			};

			if (field is IUnboundField)
				r.ExtendedProperties.Add("unbound", true);

			if (field is IBoundField bf)
			{
				if (!string.IsNullOrWhiteSpace(bf.Mapping))
					r.ExtendedProperties.Add("mapping", bf.Mapping);
			}
			return r;
		}

		private IDataCommandDescriptor CreateCommand(IExecutionContextState sender, IDataSource ds, IConnection connection, JObject arguments)
		{
			var r = new DataCommandDescriptor
			{
				CommandText = ds.CommandText.Trim(),
				CommandType = ds.CommandType,
				CommandTimeout = ds.CommandTimeout,
				ConnectionString = connection.Value.Trim()
			};

			ValidateArguments(sender, ds.Parameters.ToList(), arguments);
			SetCommandParameters(sender, r, ds.Parameters.ToList(), arguments);

			return r;
		}

		public IDataConnection OpenConnection(Guid microService, string connection)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent(microService, "Connection", connection);

			if (component == null)
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection), CreateDescriptor(ExecutionEvents.OpenConnection, ExecutionContextState.Connection, null, null, microService));

			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IConnection config))
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection), CreateDescriptor(ExecutionEvents.OpenConnection, ExecutionContextState.Connection, null, null, microService));

			var d = CreateDescriptor(ExecutionEvents.OpenConnection, ExecutionContextState.Connection, component.Token.ToString(), null, microService);

			var con = CreateConnection(Context, d, config.Component, component.Name);
			var dataProvider = CreateDataProvider(Context, d, config);

			return dataProvider.OpenConnection(config.Value);
		}
	}
}
