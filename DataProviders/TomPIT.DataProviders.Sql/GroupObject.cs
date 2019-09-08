using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
{
	internal class GroupObject : IGroupObject
	{
		public string Text {get;set;}

		public string Value {get;set;}

		public string Description {get;set;}
	}
}
