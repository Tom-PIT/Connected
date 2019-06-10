using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.ComponentModel.Apis
{
	[ExceptionSourceProperty(nameof(ExceptionSource))]
	public abstract class OperationArguments : EventArguments, IApiExecutionScope, IDataModelContext
	{
		internal const string DataSourceProvider = "TomPIT.Design.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design";
		internal const string DataSourceParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.DataSourceParameterProvider, TomPIT.Design";
		internal const string TransactionProvider = "TomPIT.Design.CodeAnalysis.Providers.TransactionProvider, TomPIT.Design";
		internal const string TransactionParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.TransactionParameterProvider, TomPIT.Design";
		internal const string ConnectionProvider = "TomPIT.Design.CodeAnalysis.Providers.ConnectionProvider, TomPIT.Design";
		private const string EventProvider = "TomPIT.Design.CodeAnalysis.Providers.EventProvider, TomPIT.Design";

		private DatabaseGet _database = null;
		private DatabasePost _databasePost = null;

		public OperationArguments(IExecutionContext sender, IApiOperation operation, JObject arguments) : base(sender)
		{
			Arguments = arguments ?? new JObject();
			Operation = operation;
		}

		public JObject Arguments { get; }
		protected IApiOperation Operation { get; }

		public IApi Api
		{
			get { return Operation.Closest<IApi>(); }
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

		protected override string ExceptionSource
		{
			get
			{
				return Api == null || Operation == null
					? GetType().ShortName()
					: string.Format("{0}/{1}", Api.ComponentName(this), Operation.Name);
			}
		}

		public Guid Event<T>([CodeAnalysisProvider(EventProvider)]string name, T e)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, null, e);
		}

		public Guid Event([CodeAnalysisProvider(EventProvider)]string name)
		{
			return Event(name, null);
		}

		public Guid Event<T>([CodeAnalysisProvider(EventProvider)]string name, T e, IEventCallback callback)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, callback, e);
		}

		public Guid Event([CodeAnalysisProvider(EventProvider)]string name, IEventCallback callback)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, callback);
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