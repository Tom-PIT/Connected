using System;
using System.Diagnostics;

namespace TomPIT.Diagnostics
{
	internal class LogEntry : ILogEntry
	{
		public string Category { get; set; }
		public string Message { get; set; }
		public TraceLevel Level { get; set; }
		public string Source { get; set; }
		public DateTime Created { get; set; }
		public int EventId { get; set; }
		public Guid MicroService { get; set; }
		public string AuthorityId { get; set; }
		public string Authority { get; set; }
		public string ContextAuthority { get; set; }
		public string ContextAuthorityId { get; set; }
		public Guid ContextMicroService { get; set; }
		public string ContextProperty { get; set; }
		public long Id { get; set; }
	}
}
