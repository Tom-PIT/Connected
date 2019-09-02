using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Development.Navigation
{
	public interface INavigationRouteDescriptor
	{
		string RouteKey { get; }
		string Template { get; }
		string Text { get; }
	}
}
