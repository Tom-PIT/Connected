using System;
using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface ITimezoneHandler
	{
		ITimezone Select(Guid token);
		List<ITimezone> Query();
		void Insert(Guid token, string name, int offset);
		void Update(ITimezone timezone, string name, int offset);
		void Delete(ITimezone timezone);
	}
}
