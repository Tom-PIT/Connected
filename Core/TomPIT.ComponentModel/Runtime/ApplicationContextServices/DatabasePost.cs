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
	internal class DatabasePost : Database
	{
		public DatabasePost(IApplicationContext context) : base(context)
		{

		}

		public JObject Execute(Guid microService, string name, JObject arguments, IDataConnection connection)
		{
			var ctx = new ApplicationContext(Context);

			ctx.Identity.SetContextId(microService.ToString());

			var component = ctx.GetServerContext().GetService<IComponentService>().SelectComponent(microService, "Transaction", name);

			if (component == null)
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrTransactionNotFound, name), CreateDescriptor(RuntimeEvents.DataWrite, ExecutionContext.DatabasePost, null, null, microService));

			if (!(ctx.GetServerContext().GetService<IComponentService>().SelectConfiguration(component.Token) is ITransaction config))
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrTransactionNotFound, name), CreateDescriptor(RuntimeEvents.DataWrite, ExecutionContext.DatabasePost, null, null, microService));

			var d = CreateDescriptor(RuntimeEvents.DataWrite, ExecutionContext.DatabasePost, component.Token.ToString(), null, microService);

			var con = CreateConnection(ctx, d, config.Connection, component.Name);
			var dataProvider = CreateDataProvider(ctx, d, con);

			var preparing = ctx.GetServerContext().Execute(config.Preparing, this, new TransactionPreparingArguments(ctx, arguments));

			if (preparing.Cancel)
				return null;

			var command = CreateCommand(d, config, con, preparing.Arguments);

			var validating = ctx.GetServerContext().Execute(config.Validating, this, new ValidatingArguments(ctx, command));

			if (validating.ValidationErrors.Count > 0)
				throw new ServerValidationException(validating.ValidationErrors);

			var executing = ctx.GetServerContext().Execute(config.Executing, this, new TransactionExecutingArguments(ctx, command));

			if (executing.Cancel)
				return null;

			dataProvider.Execute(command, connection);

			var returnValues = new JObject();

			foreach (var i in command.Parameters)
			{
				if (i.Direction == ParameterDirection.ReturnValue)
				{
					if (i.Value == null || i.Value == DBNull.Value)
						returnValues.Add(i.Name, JValue.CreateNull());
					else
						returnValues.Add(i.Name, JToken.FromObject(i.Value));
				}
			}

			var executed = ctx.GetServerContext().Execute(config.Executed, this, new TransactionExecutedArguments(ctx, returnValues));

			return executed.ReturnValues;

		}

		public JObject Execute(Guid microService, string name, JObject arguments)
		{
			return Execute(microService, name, arguments, null);
		}

		private IDataCommandDescriptor CreateCommand(IExecutionContext sender, ITransaction ds, IConnection connection, JObject arguments)
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
	}
}
