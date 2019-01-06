using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class LoggingController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var category = body.Optional("category", string.Empty);
			var message = body.Optional("message", string.Empty);
			var level = body.Optional("level", TraceLevel.Off);
			var source = body.Optional("source", string.Empty);
			var eventId = body.Optional("eventId", 0);
			var microService = body.Optional("microService", Guid.Empty);
			var authorityId = body.Optional("authorityId", string.Empty);
			var authority = body.Optional("authority", string.Empty);
			var contextAuthority = body.Optional("contextAuthority", string.Empty);
			var contextAuthorityId = body.Optional("contextAuthorityId", string.Empty);
			var contextMicroService = body.Optional("contextMicroService", Guid.Empty);
			var contextProperty = body.Optional("contextProperty", string.Empty);

			DataModel.Logging.Insert(category, source, message, level, eventId, microService, contextMicroService,
				authorityId, authority, contextAuthority, contextAuthorityId, contextProperty);
		}
	}
}
