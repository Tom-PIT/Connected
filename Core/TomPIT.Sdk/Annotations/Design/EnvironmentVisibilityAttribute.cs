﻿using System;
using TomPIT.Runtime;

namespace TomPIT.Annotations.Design
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