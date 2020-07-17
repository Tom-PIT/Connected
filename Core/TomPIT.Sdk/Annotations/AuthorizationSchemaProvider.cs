using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Annotations
{
	public class AuthorizationSchemaProvider : IAuthorizationSchemaProvider
	{
		public string RootKey { get; protected set; }

		public bool SupportsSchema { get; protected set; }

		public List<string> QueryClaims()
		{
			return OnQueryClaims();
		}

		protected virtual List<string> OnQueryClaims()
		{
			return new List<string>();
		}

		public List<IAuthorizationSchemaElement> QuerySchema(string parent)
		{
			return OnQuerySchema(parent);
		}

		protected virtual List<IAuthorizationSchemaElement> OnQuerySchema(string parent)
		{
			return new List<IAuthorizationSchemaElement>();
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
