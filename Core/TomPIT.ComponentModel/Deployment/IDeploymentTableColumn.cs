namespace TomPIT.Deployment
{
	public interface IDeploymentTableColumn : IDeploymentDatabaseEntity
	{
		string DataType { get; }
		bool IsNullable { get; }
		bool PrimaryKey { get; }
		bool Identity { get; }
	}
}
