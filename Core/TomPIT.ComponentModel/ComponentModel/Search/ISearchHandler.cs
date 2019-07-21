using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Search;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Search
{
	public interface ISearchHandler<T> : ISearchProcessHandler
	{
		List<T> Query();
	}
}
