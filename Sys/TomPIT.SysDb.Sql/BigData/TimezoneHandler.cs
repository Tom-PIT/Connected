using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TimezoneHandler : ITimezoneHandler
	{
		public void Delete(ITimezone timezone)
		{
			throw new NotImplementedException();
		}

		public void Insert(Guid token, string name, int offset)
		{
			throw new NotImplementedException();
		}

		public List<ITimezone> Query()
		{
			throw new NotImplementedException();
		}

		public ITimezone Select(Guid token)
		{
			throw new NotImplementedException();
		}

		public void Update(ITimezone timezone, string name, int offset)
		{
			throw new NotImplementedException();
		}
	}
}
