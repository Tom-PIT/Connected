namespace TomPIT.Management
{
	public class ManagementSchemaElement : IManagementSchemaElement
	{
		public string Text { get; set; }

		public int ChildrenCount { get; set; }

		public string Id { get; set; }

		public SchemaElementType Type { get; set; } = SchemaElementType.Descriptor;
	}
}
