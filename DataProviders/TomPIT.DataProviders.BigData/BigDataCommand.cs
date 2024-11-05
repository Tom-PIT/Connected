using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.DataProviders.BigData
{
	public class BigDataCommand : DbCommand
	{
		private JArray _postData = null;
		private DbConnection _connection = null;
		private DbParameterCollection _parameters = null;
		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; } = 120;
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }
		protected override DbConnection DbConnection
		{
			get { return _connection; }
			set
			{
				if (_connection != value)
				{
					_connection = value;

					((BigDataConnection)_connection).Transaction.RegisterCommand(this);
				}
			}
		}

		private JArray PostData
		{
			get
			{
				if (_postData == null)
					_postData = new JArray();

				return _postData;
			}
		}
		protected override DbParameterCollection DbParameterCollection
		{
			get
			{
				if (_parameters == null)
					_parameters = new BigDataParameters();

				return _parameters;
			}
		}

		protected override DbTransaction DbTransaction { get; set; }

		public override void Cancel()
		{
			_postData = null;
		}

		public override int ExecuteNonQuery()
		{
			if (string.IsNullOrWhiteSpace(Connection.DataSource))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceFeatures.BigData}, {InstanceVerbs.Post})");

			if (string.IsNullOrWhiteSpace(CommandText))
				throw new RuntimeException(nameof(BigDataCommand), SR.ErrCommandTextNull, LogCategories.BigData);

			var record = new JObject();

			foreach (BigDataParameter parameter in Parameters)
				record.Add(new JProperty(parameter.ParameterName, parameter.Value));

			PostData.Add(record);

			return 1;
		}

		public void Commit()
		{
			var interval = 0;

			for (var i = 0; i < 3; i++)
			{
				try
				{
					TryCommit();

					break;
				}
				catch
				{
					if (i == 2)
						throw;
					else
					{
						interval = interval == 0 ? 1 : interval *= 3;

						Thread.Sleep(interval*1000);
					}
				}
			}
		}

		private void TryCommit()
		{
			if (PostData.Count == 0)
				return;

			var tokens = CommandText.Split('/');
			var u = $"{Connection.DataSource}/data/{tokens[0]}/{tokens[1]}";

			MiddlewareDescriptor.Current.Tenant.Post(u, PostData);

			_postData = null;
		}
		public override object ExecuteScalar()
		{
			throw new NotSupportedException();
		}

		public override void Prepare()
		{

		}

		protected override DbParameter CreateDbParameter()
		{
			return new BigDataParameter();
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			return new BigDataReader(this);
		}
	}
}
