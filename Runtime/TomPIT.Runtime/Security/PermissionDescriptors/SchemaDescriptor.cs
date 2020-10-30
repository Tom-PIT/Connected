namespace TomPIT.Security.PermissionDescriptors
{
	internal class SchemaDescriptor : IPermissionSchemaDescriptor
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string Avatar { get; set; }
		public string Description { get; set; }
	}
}
