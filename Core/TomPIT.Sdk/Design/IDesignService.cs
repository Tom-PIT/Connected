namespace TomPIT.Design
{
	public interface IDesignService
	{
		IDeployment Deployment { get; }
		
		IVersionControl VersionControl { get; }
		IComponentModel Components { get; }
		IDesignSearch Search { get; }
		ITextDiff TextDiff { get; }
		IMicroServiceDesign MicroServices { get; }

		void Initialize();
	}
}
