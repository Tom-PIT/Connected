using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Middleware
{
	public interface IMiddlewareObject
	{
		IMiddlewareContext Context { get; set; }
	}
}
