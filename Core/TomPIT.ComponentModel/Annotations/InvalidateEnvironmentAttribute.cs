using System;
using TomPIT.Ide;

namespace TomPIT.Annotations
{
	public class InvalidateEnvironmentAttribute : Attribute
	{
		public InvalidateEnvironmentAttribute(EnvironmentSection section)
		{
			Sections = section;
		}

		public EnvironmentSection Sections { get; private set; }
	}
}