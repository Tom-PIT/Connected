using System;
using System.Data;
using System.Data.Common;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Proxy;

namespace TomPIT.DataProviders.BigData
{
	internal class BigDataConnection : DbConnection
	{
		private ConnectionState _state = ConnectionState.Closed;
		private string _dataSource = null;

		public override string ConnectionString { get; set; }

		public override string Database => null;

		public override string DataSource
		{
			get
			{
				if (_dataSource == null)
				{
					if (string.IsNullOrWhiteSpace(ConnectionString) || string.Compare(ConnectionString, "local", true) == 0)
						_dataSource = MiddlewareDescriptor.Current.Tenant.GetService<IInstanceEndpointService>().Url(InstanceFeatures.BigData, InstanceVerbs.Post);
					else
						_dataSource = ConnectionString;
				}

				return _dataSource;
			}
		}

		public override string ServerVersion => null;

		public override ConnectionState State => _state;

		public override void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public override void Close()
		{
			_state = ConnectionState.Closed;
		}

		public override void Open()
		{
			_state = ConnectionState.Open;
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			Transaction = new BigDataTransaction(this, isolationLevel);

			return Transaction;
		}

		internal BigDataTransaction Transaction { get; private set; }

		internal IBigDataProxy ResolveProxy()
		{
			var proxy = MiddlewareDescriptor.Current.Tenant.GetService<IBigDataProxy>();

			if (proxy != null)
			{
				var ownUrl = MiddlewareDescriptor.Current.Tenant.GetService<IInstanceEndpointService>()
					?.Url(InstanceFeatures.BigData, InstanceVerbs.Post);

				if (string.Equals(DataSource, ownUrl, StringComparison.OrdinalIgnoreCase))
					return proxy;
			}

			return new RemoteBigDataProxy(DataSource);
		}

		protected override DbCommand CreateDbCommand()
		{
			return new BigDataCommand
			{
				Connection = this
			};
		}
	}
}
