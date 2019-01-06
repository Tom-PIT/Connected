using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : ILoggingService
	{
		public LoggingService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Write(ILogEntry d)
		{
			var u = Connection.CreateUrl("Logging", "Insert");
			var args = new JObject
			{
				{ "category",d.Category },
				{"message",d.Message },
				{"level",d.Level.ToString()},
				{"source",d.Source},
				{"eventId",d.EventId},
				{"microService",d.MicroService},
				{"authorityId",d.AuthorityId},
				{"authority",d.Authority},
				{"contextAuthority",d.ContextAuthority},
				{"contextAuthorityId",d.ContextAuthorityId},
				{"contextMicroService", d.ContextMicroService},
				{"contextProperty",d.ContextProperty}
			};

			Connection.Post(u, args);
		}
	}
}
