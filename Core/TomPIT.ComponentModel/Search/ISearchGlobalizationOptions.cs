using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Search
{
	public interface ISearchGlobalizationOptions
	{
		int Lcid { get; set; }
		int UICulture { get; set; }
	}
}
