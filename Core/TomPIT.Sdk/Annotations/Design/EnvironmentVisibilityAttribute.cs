using System;
using TomPIT.Runtime;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	[Obsolete("This feature is obsolete now and will be removed in the next release.")]
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
