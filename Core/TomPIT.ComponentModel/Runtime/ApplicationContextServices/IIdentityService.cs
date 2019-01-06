using TomPIT.Security;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface IIdentityService
	{
		bool IsAuthenticated { get; }
		IUser User { get; }

		IUser GetUser(object qualifier);
	}
}
