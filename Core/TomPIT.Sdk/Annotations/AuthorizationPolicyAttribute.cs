using System;

public enum AuthorizationPolicyBehavior
{
	Mandatory = 1,
	Optional = 2
}

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class AuthorizationPolicyAttribute : Attribute
	{
		public AuthorizationPolicyAttribute(Type policyType, string policy) : this(policyType, policy, 0)
		{
		}

		public AuthorizationPolicyAttribute(Type policyType, string policy, int priority) : this(policyType, policy, priority, AuthorizationPolicyBehavior.Mandatory)
		{
		}

		public AuthorizationPolicyAttribute(Type policyType, string policy, int priority, AuthorizationPolicyBehavior behavior)
		{
			PolicyType = policyType;
			Priority = priority;
			Policy = policy;
			Behavior = behavior;
		}

		public AuthorizationPolicyBehavior Behavior { get; } = AuthorizationPolicyBehavior.Mandatory;
		public Type PolicyType { get; }
		public int Priority { get; }
		public string Policy { get; }
	}
}
