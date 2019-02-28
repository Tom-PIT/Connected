using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.SysDb.Data
{
	public interface IUserDataHandler
	{
		void Update(IUser user, List<IUserData> data);
		List<IUserData> Query(IUser user);
	}
}
