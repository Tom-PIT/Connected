using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Development.Navigation
{
	internal class NavigationRouteDescriptor : INavigationRouteDescriptor
	{
		public string RouteKey {get;set;}
		public string Template {get;set;}
		public string Text {get;set;}
	}
}
