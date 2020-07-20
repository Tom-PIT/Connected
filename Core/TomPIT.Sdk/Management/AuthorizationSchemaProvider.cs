using System.Collections.Generic;
using System.Linq;
using TomPIT.Annotations;

namespace TomPIT.Management
{
	public class AuthorizationSchemaProvider : ManagementSchemaProvider, IAuthorizationSchemaProvider
	{

		public List<string> QueryClaims()
		{
			return OnQueryClaims();
		}

		protected virtual List<string> OnQueryClaims()
		{
			return new List<string>();
		}

		protected List<string> HigherThan(object value)
		{
			return AuthorizationPolicyAttribute.SelectEnumValues(value, EnumOperation.HigherThan)?.ToList();
		}

		protected List<string> AtLeast(object value)
		{
			return AuthorizationPolicyAttribute.SelectEnumValues(value, EnumOperation.AtLeast)?.ToList();
		}


	}
}
