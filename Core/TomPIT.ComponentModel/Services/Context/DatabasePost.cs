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
	internal class DatabasePost : Database
	{
		public DatabasePost(IExecutionContext context) : base(context)
		{

		}

		public JObject Execute(Guid microService, string name, JObject arguments, IDataConnection connection)
		{
			var ctx = new ExecutionContext(Context);

			ctx.Identity.SetContextId(microService.ToString());

			var component = ctx.Connection().GetService<IComponentService>().SelectComponent(microService, "Transaction", name);

			if (component == null)
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrTransactionNotFound, name), CreateDescriptor(ExecutionEvents.DataWrite, ExecutionContextState.DatabasePost, null, null, microService));

			if (!(ctx.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is ITransaction config))
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrTransactionNotFound, name), CreateDescriptor(ExecutionEvents.DataWrite, ExecutionContextState.DatabasePost, null, null, microService));

			var d = CreateDescriptor(ExecutionEvents.DataWrite, ExecutionContextState.DatabasePost, component.Token.ToString(), null, microService);

			var con = CreateConnection(ctx, d, config.Connection, component.Name);
			var dataProvider = CreateDataProvider(ctx, d, con);

			var preparing = ctx.Connection().Execute(config.Preparing, this, new TransactionPreparingArguments(ctx, arguments));

			if (preparing.Cancel)
				return null;

			var command = CreateCommand(d, config, con, preparing.Arguments);

			var validating = ctx.Connection().Execute(config.Validating, this, new ValidatingArguments(ctx, command));

			if (validating.ValidationErrors.Count > 0)
				throw new ValidationException(validating.ValidationErrors);

			var executing = ctx.Connection().Execute(config.Executing, this, new TransactionExecutingArguments(ctx, command));

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

			var executed = ctx.Connection().Execute(config.Executed, this, new TransactionExecutedArguments(ctx, returnValues));

			return executed.ReturnValues;

		}

		public JObject Execute(Guid microService, string name, JObject arguments)
		{
			return Execute(microService, name, arguments, null);
		}

		private IDataCommandDescriptor CreateCommand(IExecutionContextState sender, ITransaction ds, IConnection connection, JObject arguments)
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
