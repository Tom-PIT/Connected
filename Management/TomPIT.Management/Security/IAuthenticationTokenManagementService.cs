using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	public interface IAuthenticationTokenManagementService
	{
		Guid Insert(Guid resourceGroup, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions);
		void Update(Guid token, Guid user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions);
		void Delete(Guid token);

		List<IAuthenticationToken> Query(string resourceGroup);
		IAuthenticationToken Select(Guid token);
	}
}
