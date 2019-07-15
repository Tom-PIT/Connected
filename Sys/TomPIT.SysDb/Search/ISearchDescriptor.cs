using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.SysDb.Search
{
	public interface ISearchDescriptor
	{
		Guid Identifier { get; }
		string Name { get; }
		DateTime Created { get; }
		string Arguments { get; }
		Guid MicroService { get; }
	}
}
