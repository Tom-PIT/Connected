using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class MappingAttribute : Attribute
	{
		public MappingAttribute(string dataSourceField)
		{
			DataSourceField = dataSourceField;
		}

		public string DataSourceField { get; }
	}
}
