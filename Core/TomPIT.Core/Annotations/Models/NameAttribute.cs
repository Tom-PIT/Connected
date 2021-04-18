using System;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class NameAttribute : Attribute
	{
		public NameAttribute(string columnName)
		{
			ColumnName = columnName;
		}

		public string ColumnName { get; }
	}
}
