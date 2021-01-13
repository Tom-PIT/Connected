namespace TomPIT.Design
{
	public interface IDesignService
	{
		IVersionControl VersionControl { get; }
		IComponentModel Components { get; }
		IDesignSearch Search { get; }
	}
}
