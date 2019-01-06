using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Diagnostics
{
	internal class LoggingService : ILoggingService
	{
		public LoggingService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Write(ILogEntry d)
		{
			var u = Server.CreateUrl("Logging", "Insert");
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

			Server.Connection.Post(u, args);
		}
	}
}
