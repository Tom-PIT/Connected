using System;

namespace TomPIT.Data
{
	public interface IAuditDescriptor
	{
		Guid User { get; }
		DateTime Created { get; }
		string PrimaryKey { get; }
		string Category { get; }
		string Event { get; }
		string Description { get; }
		string Ip { get; }
		string Property { get; }
		string Value { get; }
		Guid Identifier { get; }
	}
}
