using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TomPIT.Search
{
	internal class GlobalizationOptions : ISearchGlobalizationOptions
	{
		public int Lcid { get; set; }
		public int UICulture {get;set;}
	}
}
