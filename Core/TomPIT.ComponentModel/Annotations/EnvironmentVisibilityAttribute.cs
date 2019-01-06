using System;
using TomPIT.Services;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class EnvironmentVisibilityAttribute : Attribute
	{
		public EnvironmentVisibilityAttribute()
		{

		}

		public EnvironmentVisibilityAttribute(EnvironmentMode visibility)
		{
			Visibility = visibility;
		}

		public EnvironmentMode Visibility { get; } = EnvironmentMode.Design;
	}
}
