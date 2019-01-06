using TomPIT.Security;

namespace TomPIT.Services.Context
{
	public interface IContextIdentityService
	{
		bool IsAuthenticated { get; }
		IUser User { get; }

		IUser GetUser(object qualifier);
	}
}
