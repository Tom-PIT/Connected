using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Navigation
{
	public interface ISiteMapContextElement
	{
		IDataModelContext Context { get; set; }
	}
}
