using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.Data.DataProviders;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.ComponentModel.Apis
{
	[ExceptionSourceProperty(nameof(ExceptionSource))]
	public abstract class OperationArguments : EventArguments, IApiExecutionScope
	{
		private DatabaseGet _database = null;
		private DatabasePost _databasePost = null;

		public OperationArguments(IExecutionContext sender, IApiOperation operation, JObject arguments) : base(sender)
		{
			Arguments = arguments ?? new JObject();
			Operation = operation;
		}

		public JObject Arguments { get; }
		protected IApiOperation Operation { get; }

		public RuntimeException Exception(string message)
		{
			return new RuntimeException(Api.ComponentName(this), message);
		}

		public RuntimeException Exception(string format, string message)
		{
			return new RuntimeException(Api.ComponentName(this), string.Format("{0}", message));
		}

		public IApi Api
		{
			get { return Operation.Closest<IApi>(); }
		}

		public JObject DataSource([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design")]string dataSource,
			[CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceParameterProvider, TomPIT.Design")]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);

			return (JObject)DatabaseGet(q, arguments);
		}

		public JObject DataSource([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design")]string dataSource)
		{
			return DataSource(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design")]string dataSource,
			[CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceParameterProvider, TomPIT.Design")]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);
			var r = DatabaseGet(q, arguments);

			if (r == null)
				return default(T);

			return (T)r;
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design")]string dataSource)
		{
			return DataSource<T>(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceProvider, TomPIT.Design")]string dataSource,
			[CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.DataSourceParameterProvider, TomPIT.Design")]JObject arguments, T defaultValue)
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

		public JObject Transaction([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.TransactionProvider, TomPIT.Design")]string transaction,
			[CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.TransactionParameterProvider, TomPIT.Design")]JObject arguments)
		{
			return Transaction(null, transaction, arguments);
		}

		public JObject Transaction([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.TransactionProvider, TomPIT.Design")]string transaction)
		{
			return Transaction(transaction, null);
		}

		public JObject Transaction(IDataConnection connection, [CodeAnalysisProvider("TomPIT.Design.TransactionProvider, TomPIT.Development")]string transaction,
			[CodeAnalysisProvider("TomPIT.Design.TransactionParameterProvider, TomPIT.Development")]JObject arguments)
		{
			var q = new DataQualifier(this, transaction);

			return DatabasePost(q, arguments, connection);
		}

		public JObject Transaction(IDataConnection connection, [CodeAnalysisProvider("TomPIT.Design.TransactionProvider, TomPIT.Development")]string transaction)
		{
			return Transaction(connection, transaction, null);
		}

		public IDataConnection OpenConnection([CodeAnalysisProvider("TomPIT.Design.ConnectionProvider, TomPIT.Development")]string connection)
		{
			return DatabaseRead.OpenConnection(this.MicroService(), connection);
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

		private string ExceptionSource
		{
			get
			{
				return Api == null || Operation == null
					? GetType().ShortName()
					: string.Format("{0}/{1}", Api.ComponentName(this), Operation.Name);
			}
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.EventProvider, TomPIT.Design")]string name, JObject e)
		{
			return Connection.GetService<IEventService>().Trigger(this.MicroService(), name, e, null);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.EventProvider, TomPIT.Design")]string name)
		{
			return Event(name, null, null);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.EventProvider, TomPIT.Design")]string name, JObject e, IEventCallback callback)
		{
			return Connection.GetService<IEventService>().Trigger(this.MicroService(), name, e, callback);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.CodeAnalysis.Providers.EventProvider, TomPIT.Design")]string name, IEventCallback callback)
		{
			return Event(name, null, callback);
		}
	}
}