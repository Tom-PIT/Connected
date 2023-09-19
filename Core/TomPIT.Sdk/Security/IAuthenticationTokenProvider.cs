using TomPIT.Environment;

namespace TomPIT.Security
{
	public interface IAuthenticationTokenProvider
	{
		string RequestToken(InstanceFeatures features);
	}
}
