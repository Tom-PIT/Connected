using System;
using TomPIT.Security;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class AnonymousAuthorizationAttribute : AuthorizationPolicyAttribute
	{
		protected override IAuthorizationModel OnCreateModel()
		{
			return new AnonymousAuthorizationModel();
		}

		public class AnonymousAuthorizationModel : AuthorizationModel
		{

		}
	}
}
