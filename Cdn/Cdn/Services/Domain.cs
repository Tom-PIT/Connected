using System;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Cdn.Services
{
	internal class Domain : IDisposable
	{
		private const int ConnectionLimit = 5;

		private List<SmtpConnection> _connections = null;
		private object _sync = new object();
		private bool _disposing = false;

		public Domain(string domainName)
		{
			DomainName = domainName;
		}

		public SmtpConnection Request()
		{
			if (_disposing)
				return null;

			Timestamp = DateTime.UtcNow;

			lock (_sync)
			{
				var connection = Connections.FirstOrDefault(f => f.State == ConnectionState.Idle);

				if (connection != null)
					return connection;

				if (Connections.Count >= ConnectionLimit)
					return null;

				connection = new SmtpConnection(DomainName);

				Connections.Add(connection);

				return connection;
			}
		}

		public void Dispose()
		{
			_disposing = true;

			lock (_sync)
			{
				foreach (var connection in Connections)
					connection.Dispose();

				Connections.Clear();
			}
		}

		public bool IsDisposable
		{
			get
			{
				lock (_sync)
				{
					var hasActives = Connections.Count(f => f.State == ConnectionState.Active) > 0;

					return !hasActives && Timestamp.AddMinutes(5) < DateTime.UtcNow;
				}
			}
		}

		private DateTime Timestamp { get; set; } = DateTime.UtcNow;
		private string DomainName { get; }

		private List<SmtpConnection> Connections
		{
			get
			{
				if (_connections == null)
					_connections = new List<SmtpConnection>();

				return _connections;
			}
		}
	}
}
