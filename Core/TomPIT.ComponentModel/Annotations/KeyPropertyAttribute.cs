using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class KeyPropertyAttribute : Attribute
	{
		public KeyPropertyAttribute()
		{
		}
	}
}
