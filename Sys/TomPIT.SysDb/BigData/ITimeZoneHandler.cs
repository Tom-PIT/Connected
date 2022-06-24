using System;
using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface ITimeZoneHandler
	{
		ITimeZone Select(Guid token);
		List<ITimeZone> Query();
		void Insert(Guid token, string name, int offset);
		void Update(ITimeZone timezone, string name, int offset);
		void Delete(ITimeZone timezone);
	}
}
