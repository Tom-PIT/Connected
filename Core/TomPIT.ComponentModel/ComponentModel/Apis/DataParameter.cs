using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TomPIT.Data;

namespace TomPIT.ComponentModel.Apis
{
	internal class DataParameter : TomPIT.Data.IDataParameter
	{
		public string Name {get;set;}
		public object Value {get;set;}
		public ParameterDirection Direction {get;set;}
	}
}
