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
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name))
				{
				}.WithMetrics(ctx);
			}

			if (!(ctx.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IDataSource config))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name))
				{
					Component = component.Token
				}.WithMetrics(ctx);
			}

			var schema = CreateSchema(component, config);

			var connection = CreateConnection(ctx, config.Connection, config);
			var dataProvider = CreateDataProvider(ctx, connection);

			var preparing = ctx.Connection().Execute(config.Preparing, this, new PreparingArguments(ctx, arguments, schema));

			if (preparing.Cancel)
				return new JObject();

			var command = CreateCommand(config, connection, preparing.Arguments);

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

		private IDataCommandDescriptor CreateCommand(IDataSource ds, IConnection connection, JObject arguments)
		{
			var r = new DataCommandDescriptor
			{
				CommandText = ds.CommandText.Trim(),
				CommandType = ds.CommandType,
				CommandTimeout = ds.CommandTimeout,
				ConnectionString = connection.Value.Trim()
			};

			ValidateArguments(ds, ds.Parameters.ToList(), arguments);
			SetCommandParameters(r, ds.Parameters.ToList(), arguments);

			return r;
		}

		public IDataConnection OpenConnection(Guid microService, string connection)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent(microService, "Connection", connection);

			if (component == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection)).WithMetrics(Context);

			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IConnection config))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection))
				{
					Component = component.Token
				}.WithMetrics(Context);
			}

			var con = CreateConnection(Context, component.Token, config);
			var dataProvider = CreateDataProvider(Context, config);

			return dataProvider.OpenConnection(config.Value);
		}
	}
}
