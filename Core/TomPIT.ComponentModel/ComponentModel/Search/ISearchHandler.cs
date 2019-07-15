using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Search
{
	public interface ISearchHandler<T> : IProcessHandler
	{
		List<T> Query();
	}
}
