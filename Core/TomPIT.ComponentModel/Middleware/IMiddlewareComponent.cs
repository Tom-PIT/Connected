using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Middleware
{
	public interface IMiddlewareComponent
	{
		IDataModelContext Context { get; set; }
	}
}
