using System;
using TomPIT.Data;

namespace TomPIT.Proxy.Remote
{
	internal class AuditDescriptor : IAuditDescriptor
	{
		public Guid User { get; set; }
		public DateTime Created { get; set; }
		public string PrimaryKey { get; set; }
		public string Category { get; set; }
		public string Event { get; set; }
		public string Description { get; set; }
		public string Ip { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }
		public Guid Identifier { get; set; }
	}
}
