namespace TomPIT.SysDb.Development
{
	public interface IDevelopmentHandler
	{
		IFolderHandler Folders { get; }
		IComponentHandler Components { get; }
		IQaHandler QA { get; }
	}
}
