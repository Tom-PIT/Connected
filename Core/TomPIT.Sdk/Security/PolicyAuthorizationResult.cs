using TomPIT.Annotations;

namespace TomPIT.Security
{
	public class PolicyAuthorizationResult
	{
		public PolicyAuthorizationResult()
		{

		}
		public PolicyAuthorizationResult(string message)
		{
			Message = message;
		}

		public string Message { get; set; }

		public static PolicyAuthorizationResult Default(AuthorizationPolicyAttribute sender, object policy)
		{
			return new PolicyAuthorizationResult($"{SR.PolicyAuthorizationFailed} ({sender.GetType().Name}.{policy})");
		}
	}
}
