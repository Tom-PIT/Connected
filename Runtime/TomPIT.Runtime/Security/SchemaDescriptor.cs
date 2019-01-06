using System;

namespace TomPIT.Security
{
	internal class SchemaDescriptor : IPermissionSchemaDescriptor
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Avatar { get; set; }
		public string Description { get; set; }
	}
}
