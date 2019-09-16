using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	public class KeyPropertyAttribute : Attribute
	{
		public KeyPropertyAttribute()
		{
		}
	}
}
