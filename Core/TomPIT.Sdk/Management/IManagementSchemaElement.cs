namespace TomPIT.Management
{
	public enum SchemaElementType
	{
		Container = 1,
		Descriptor = 2
	}

	public interface IManagementSchemaElement
	{
		string Text { get; }
		int ChildrenCount { get; }
		string Id { get; }
		SchemaElementType Type { get; }
	}
}
