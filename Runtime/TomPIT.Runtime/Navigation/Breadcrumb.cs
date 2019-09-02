using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Navigation
{
	internal class Breadcrumb : IBreadcrumb
	{
		public string Text {get;set;}

		public string Key {get;set;}

		public string Url {get;set;}
	}
}
