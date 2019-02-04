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
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrTransactionNotFound, name)).WithMetrics(ctx);
			}

			if (!(ctx.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is ITransaction config))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrTransactionNotFound, name))
				{
					Component = component.Token
				}.WithMetrics(ctx);
			}

			var con = CreateConnection(ctx, config.Connection, config.Configuration());
			var dataProvider = CreateDataProvider(ctx, con);
			var metric = ctx.Services.Diagnostic.StartMetric(config.Metrics, arguments);
			JObject dr = null;

			try
			{
				var preparing = ctx.Connection().Execute(config.Preparing, this, new TransactionPreparingArguments(ctx, arguments));

				if (preparing.Cancel)
					return null;

				var command = CreateCommand(config, con, preparing.Arguments);

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
				dr = executed.ReturnValues;

				return executed.ReturnValues;
			}
			catch (Exception ex)
			{
				ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Fail, new JObject
				{
					{"exception", ex.Message }
				});

				throw ex;
			}
			finally
			{
				ctx.Services.Diagnostic.StopMetric(metric, Diagnostics.SessionResult.Success, dr);
			}
		}

		public JObject Execute(Guid microService, string name, JObject arguments)
		{
			return Execute(microService, name, arguments, null);
		}

		private IDataCommandDescriptor CreateCommand(ITransaction ds, IConnection connection, JObject arguments)
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
	}
}
