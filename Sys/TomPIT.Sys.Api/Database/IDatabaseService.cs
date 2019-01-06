using TomPIT.SysDb;

namespace TomPIT.Sys.Api.Database
{
	public interface IDatabaseService
	{
		ISysDbProxy Proxy { get; }
	}
}
