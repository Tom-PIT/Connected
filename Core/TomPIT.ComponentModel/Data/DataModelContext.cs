using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.Data
{
	public class DataModelContext : EventArguments, IDataModelContext
	{
		internal const string ConnectionProvider = "TomPIT.Design.CodeAnalysis.Providers.ConnectionProvider, TomPIT.Design";
		internal const string CommandTextProvider = "TomPIT.Design.CodeAnalysis.Providers.CommandTextProvider, TomPIT.Design";

		public DataModelContext(IExecutionContext sender) : base(sender)
		{
		}

		public IDataConnection OpenConnection([CodeAnalysisProvider(ConnectionProvider)]string connection)
		{
			var tokens = connection.Split('/');
			var ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]);
			var component = Connection.GetService<IComponentService>().SelectComponent(ms.Token, "Connection", tokens[1]);

			if (component == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection)).WithMetrics(this);

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(component.Token) is IConnection config))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionNotFound, connection))
				{
					Component = component.Token
				}.WithMetrics(this);
			}

			var dataProvider = CreateDataProvider(config);

			return dataProvider.OpenConnection(config.Value);
		}

		public IDataReader<T> OpenReader<T>(IDataConnection connection, string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataWriter OpenWriter(IDataConnection connection, string commandText)
		{
			return new DataWriter(this)
			{
				Connection = connection,
				CommandText = commandText
			};
		}

		public IDataReader<T> OpenReader<T>([CodeAnalysisProvider(ConnectionProvider)]string connection, string commandText)
		{
			return new DataReader<T>(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText,
				CloseConnection = true
			};
		}

		public IDataWriter OpenWriter([CodeAnalysisProvider(ConnectionProvider)]string connection, string commandText)
		{
			return new DataWriter(this)
			{
				Connection = OpenConnection(connection),
				CommandText = commandText,
				CloseConnection = true
			};
		}

		/// <summary>
		/// This method creates data provider for the valid connection.
		/// </summary>
		/// <param name="connection">A connection instance which holds the information about its data provider</param>
		/// <returns>IDataProvider instance is a valid one is found.</returns>
		protected IDataProvider CreateDataProvider(IConnection connection)
		{
			/*
			 * Connection is not properly configured. We just notify the user about the issue.
			 */
			if (connection.DataProvider == Guid.Empty)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, connection.ComponentName(Connection)))
				{
					Component = connection.Component,
					EventId = ExecutionEvents.OpenConnection,
				};
			}

			var provider = Connection.GetService<IDataProviderService>().Select(connection.DataProvider);
			/*
			 * Connection has invalid data provider set. This can be for various reasons:
			 * --------------------------------------------------------------------------
			 * - data provider has been removed from the configuration
			 * - package misbehavior
			 */
			if (provider == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, provider.Name))
				{
					Component = connection.Component,
					EventId = ExecutionEvents.OpenConnection
				};
			}

			return provider;
		}
	}
}
