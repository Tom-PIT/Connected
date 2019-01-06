using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.DataProviders;
using TomPIT.Exceptions;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class DatabaseGet : Database
	{

		public DatabaseGet(IApplicationContext context) : base(context)
		{

		}

		public JObject Execute(Guid microService, string name, JObject arguments)
		{
			var ctx = new ApplicationContext(Context);

			ctx.Identity.SetContextId(microService.ToString());

			var component = ctx.GetServerContext().GetService<IComponentService>().SelectComponent(microService, "DataSource", name);

			if (component == null)
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name), CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.DatabaseGet, null, null, microService));

			if (!(ctx.GetServerContext().GetService<IComponentService>().SelectConfiguration(component.Token) is IDataSource config))
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrDataSourceNotFound, name), CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.DatabaseGet, null, null, microService));

			var schema = CreateSchema(component, config);

			var d = CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.DatabaseGet, component.Token.ToString(), null, microService);

			var connection = CreateConnection(ctx, d, config.Connection, component.Name);
			var dataProvider = CreateDataProvider(ctx, d, connection);

			var preparing = ctx.GetServerContext().Execute(config.Preparing, this, new PreparingArguments(ctx, arguments, schema));

			if (preparing.Cancel)
				return new JObject();

			var command = CreateCommand(d, config, connection, preparing.Arguments);

			var validating = ctx.GetServerContext().Execute(config.Validating, this, new ValidatingArguments(ctx, command));

			if (validating.ValidationErrors.Count > 0)
				throw new ServerValidationException(validating.ValidationErrors);

			var executing = ctx.GetServerContext().Execute(config.Executing, this, new ExecutingArguments(ctx, command, preparing.Schema));

			if (executing.Cancel)
				return new JObject();

			var result = dataProvider.Query(command, executing.Schema);

			var returnValues = new JObject();

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
					returnValues.Add(i.Name, JToken.FromObject(i.Value));
			}

			var executed = ctx.GetServerContext().Execute(config.Executed, this, new ExecutedArguments(ctx, result, returnValues));

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

		private IDataCommandDescriptor CreateCommand(IExecutionContext sender, IDataSource ds, IConnection connection, JObject arguments)
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
			var component = Context.GetServerContext().GetService<IComponentService>().SelectComponent(microService, "Connection", connection);

			if (component == null)
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection), CreateDescriptor(RuntimeEvents.OpenConnection, ExecutionContext.Connection, null, null, microService));

			if (!(Context.GetServerContext().GetService<IComponentService>().SelectConfiguration(component.Token) is IConnection config))
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection), CreateDescriptor(RuntimeEvents.OpenConnection, ExecutionContext.Connection, null, null, microService));

			var d = CreateDescriptor(RuntimeEvents.OpenConnection, ExecutionContext.Connection, component.Token.ToString(), null, microService);

			var con = CreateConnection(Context, d, config.Component, component.Name);
			var dataProvider = CreateDataProvider(Context, d, config);

			return dataProvider.OpenConnection(config.Value);
		}
	}
}
