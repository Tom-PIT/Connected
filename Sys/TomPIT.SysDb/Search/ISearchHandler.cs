using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;

namespace TomPIT.SysDb.Search
{
	public interface ISearchHandler
	{
		void Insert(IMicroService microService, string name, Guid identifier, DateTime created, string arguments);
		List<ISearchDescriptor> Query();
		ISearchDescriptor Select(Guid identifier);
		void Delete(ISearchDescriptor d);
	}
}
