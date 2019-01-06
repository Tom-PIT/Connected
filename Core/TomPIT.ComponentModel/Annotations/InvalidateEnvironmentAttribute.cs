using System;

namespace TomPIT.Annotations
{
	[Flags]
	public enum EnvironmentSection
	{
		None = 0,
		Explorer = 1,
		Designer = 2,
		Selection = 4,
		Properties = 8,
		Events = 16,
		Toolbox = 32,
		Property = 64,
		All = 1024
	}

	public class InvalidateEnvironmentAttribute : Attribute
	{
		public InvalidateEnvironmentAttribute(EnvironmentSection section)
		{
			Sections = section;
		}

		public EnvironmentSection Sections { get; private set; }
	}
}