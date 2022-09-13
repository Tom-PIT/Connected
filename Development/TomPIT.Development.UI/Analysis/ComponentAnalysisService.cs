using TomPIT.Connectivity;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisService : TenantObject, IComponentAnalysisService
	{
		public ComponentAnalysisService(ITenant tenant) : base(tenant)
		{
		}
	}
}
