using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
	public interface IAuthenticationTokenHandler
	{
		void Insert(IResourceGroup resourceGroup, IUser user, Guid token, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions);
		void Update(IAuthenticationToken authToken, IUser user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions);

		void Delete(IAuthenticationToken token);

		IAuthenticationToken Select(Guid token);
		List<IAuthenticationToken> Query();
	}
}
