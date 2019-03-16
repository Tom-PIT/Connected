using System;
using System.Collections.Generic;

namespace TomPIT.Cdn.Services
{
	internal static class ConnectionPool
	{
		private const int ConcurrentLimit = 5;

		private static Lazy<Dictionary<string, SmtpConnection>> _connections = new Lazy<Dictionary<string, SmtpConnection>>();

		public static SmtpConnection Request(string domain)
		{
			lock (Connections)
			{
				for (var i = 0; i < ConcurrentLimit; i++)
				{
					var key = $"{domain}{i}";

					if (Connections.TryGetValue(key, out SmtpConnection connection))
					{
						if (connection.State == ConnectionState.Idle)
						{
							connection.State = ConnectionState.Active;

							return connection;
						}
					}
					else
					{
						var newConnection = new SmtpConnection(domain);

						newConnection.State = ConnectionState.Active;

						Connections.Add(key, newConnection);

						return newConnection;
					}
				}
			}

			return null;
		}

		public static void CleanUp()
		{
			lock (Connections)
			{
				foreach (var connection in Connections)
				{
					if (connection.Value.State == ConnectionState.Idle && connection.Value.TimeStamp.AddMinutes(5) < DateTime.UtcNow)
					{
						connection.Value.Dispose();
						Connections.Remove(connection.Key);
					}
				}
			}
		}

		private static Dictionary<string, SmtpConnection> Connections { get { return _connections.Value; } }
	}
}
