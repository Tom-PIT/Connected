namespace TomPIT.Security
{
	public interface IPermissionSchemaDescriptor
	{
		string Id { get; }
		string Title { get; }
		string Avatar { get; }
		string Description { get; }
	}
}
