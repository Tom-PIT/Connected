namespace TomPIT.Services.Context
{
	public interface IContextDataService
	{
		IContextDataAudit Audit { get; }
		IContextUserDataService User { get; }
	}
}
