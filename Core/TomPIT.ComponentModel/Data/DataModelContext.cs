using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.Data
{
	public abstract class DataModelContext : EventArguments, IDataModelContext
	{
		internal const string DataSourceProvider = "TomPIT.Design.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design";
		internal const string DataSourceParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.DataSourceParameterProvider, TomPIT.Design";
		internal const string TransactionProvider = "TomPIT.Design.CodeAnalysis.Providers.TransactionProvider, TomPIT.Design";
		internal const string TransactionParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.TransactionParameterProvider, TomPIT.Design";
		internal const string ConnectionProvider = "TomPIT.Design.CodeAnalysis.Providers.ConnectionProvider, TomPIT.Design";

		private DatabaseGet _database = null;
		private DatabasePost _databasePost = null;

		public DataModelContext(IExecutionContext sender) : base(sender)
		{
		}

		public JObject DataSource([CodeAnalysisProvider(DataSourceProvider)]string dataSource,
		[CodeAnalysisProvider(DataSourceParameterProvider)]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);

			return (JObject)DatabaseGet(q, arguments);
		}

		public JObject DataSource([CodeAnalysisProvider(DataSourceProvider)]string dataSource)
		{
			return DataSource(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider(DataSourceProvider)]string dataSource,
			[CodeAnalysisProvider(DataSourceParameterProvider)]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);
			var r = DatabaseGet(q, arguments);

			if (r == null)
				return default(T);

			return (T)r;
		}

		public T DataSource<T>([CodeAnalysisProvider(DataSourceProvider)]string dataSource)
		{
			return DataSource<T>(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider(DataSourceProvider)]string dataSource,
			[CodeAnalysisProvider(DataSourceParameterProvider)]JObject arguments, T defaultValue)
		{
			var q = new DataQualifier(this, dataSource);
			var r = DatabaseGet(q, arguments);

			if (r == null)
				return defaultValue;

			return (T)r;
		}

		private object DatabaseGet(DataQualifier q, JObject arguments)
		{
			return DatabaseRead.Execute(q.MicroService.Token, q.DataSource, arguments);
		}

		private JObject DatabasePost(DataQualifier q, JObject arguments)
		{
			return DatabaseWrite.Execute(q.MicroService.Token, q.DataSource, arguments);
		}

		private JObject DatabasePost(DataQualifier q, JObject arguments, IDataConnection connection)
		{
			return DatabaseWrite.Execute(q.MicroService.Token, q.DataSource, arguments, connection);
		}

		public JObject Transaction([CodeAnalysisProvider(TransactionProvider)]string transaction,
			[CodeAnalysisProvider(TransactionParameterProvider)]JObject arguments)
		{
			return Transaction(null, transaction, arguments);
		}

		public JObject Transaction([CodeAnalysisProvider(TransactionProvider)]string transaction)
		{
			return Transaction(transaction, null);
		}

		public JObject Transaction(IDataConnection connection, [CodeAnalysisProvider(TransactionProvider)]string transaction,
			[CodeAnalysisProvider(TransactionParameterProvider)]JObject arguments)
		{
			var q = new DataQualifier(this, transaction);

			return DatabasePost(q, arguments, connection);
		}

		public JObject Transaction(IDataConnection connection, [CodeAnalysisProvider(TransactionProvider)]string transaction)
		{
			return Transaction(connection, transaction, null);
		}

		public IDataConnection OpenConnection([CodeAnalysisProvider(ConnectionProvider)]string connection)
		{
			return DatabaseRead.OpenConnection(MicroService.Token, connection);
		}

		private DatabaseGet DatabaseRead
		{
			get
			{
				if (_database == null)
					_database = new DatabaseGet(this);

				return _database;
			}
		}

		private DatabasePost DatabaseWrite
		{
			get
			{
				if (_databasePost == null)
					_databasePost = new DatabasePost(this);

				return _databasePost;
			}
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
	}
}
