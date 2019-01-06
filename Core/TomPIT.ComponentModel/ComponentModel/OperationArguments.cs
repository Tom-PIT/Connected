using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.DataProviders;
using TomPIT.Exceptions;
using TomPIT.Runtime;
using TomPIT.Runtime.ApplicationContextServices;

namespace TomPIT.ComponentModel
{
	[ExceptionSourceProperty(nameof(ExceptionSource))]
	public abstract class OperationArguments : EventArguments, IApiExecutionScope
	{
		private DatabaseGet _database = null;
		private DatabasePost _databasePost = null;

		public OperationArguments(IApplicationContext sender, IApiOperation operation, JObject arguments) : base(sender)
		{
			Arguments = arguments ?? new JObject();
			Operation = operation;
		}

		public JObject Arguments { get; }
		protected IApiOperation Operation { get; }

		public ApiException Exception(string message)
		{
			return new ApiException(Api.ComponentName(this), message);
		}

		public ApiException Exception(string format, string message)
		{
			return new ApiException(Api.ComponentName(this), string.Format("{0}", message));
		}

		public IApi Api
		{
			get { return Operation.Closest<IApi>(); }
		}

		public JObject DataSource([CodeAnalysisProvider("TomPIT.Design.DataSourceProvider, TomPIT.Development")]string dataSource,
			[CodeAnalysisProvider("TomPIT.Design.DataSourceParameterProvider, TomPIT.Development")]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);

			return (JObject)DatabaseGet(q, arguments);
		}

		public JObject DataSource([CodeAnalysisProvider("TomPIT.Design.DataSourceProvider, TomPIT.Development")]string dataSource)
		{
			return DataSource(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.Design.DataSourceProvider, TomPIT.Development")]string dataSource,
			[CodeAnalysisProvider("TomPIT.Design.DataSourceParameterProvider, TomPIT.Development")]JObject arguments)
		{
			var q = new DataQualifier(this, dataSource);
			var r = DatabaseGet(q, arguments);

			if (r == null)
				return default(T);

			return (T)r;
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.Design.DataSourceProvider, TomPIT.Development")]string dataSource)
		{
			return DataSource<T>(dataSource, null);
		}

		public T DataSource<T>([CodeAnalysisProvider("TomPIT.Design.DataSourceProvider, TomPIT.Development")]string dataSource,
			[CodeAnalysisProvider("TomPIT.Design.DataSourceParameterProvider, TomPIT.Development")]JObject arguments, T defaultValue)
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

		public JObject Transaction([CodeAnalysisProvider("TomPIT.Design.TransactionProvider, TomPIT.Development")]string transaction,
			[CodeAnalysisProvider("TomPIT.Design.TransactionParameterProvider, TomPIT.Development")]JObject arguments)
		{
			return Transaction(null, transaction, arguments);
		}

		public JObject Transaction([CodeAnalysisProvider("TomPIT.Design.TransactionProvider, TomPIT.Development")]string transaction)
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

		public Guid Event([CodeAnalysisProvider("TomPIT.Design.EventProvider, TomPIT.Development")]string name, JObject e)
		{
			return SysContext.GetService<IEventService>().Trigger(this.MicroService(), name, e, null);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.Design.EventProvider, TomPIT.Development")]string name)
		{
			return Event(name, null, null);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.Design.EventProvider, TomPIT.Development")]string name, JObject e, IEventCallback callback)
		{
			return SysContext.GetService<IEventService>().Trigger(this.MicroService(), name, e, callback);
		}

		public Guid Event([CodeAnalysisProvider("TomPIT.Design.EventProvider, TomPIT.Development")]string name, IEventCallback callback)
		{
			return Event(name, null, callback);
		}
	}
}