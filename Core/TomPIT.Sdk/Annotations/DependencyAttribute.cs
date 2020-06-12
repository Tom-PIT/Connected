using System;

namespace TomPIT.Annotations
{
	public sealed class DependencyAttribute : Attribute
	{
		public DependencyAttribute(string model)
		{
			Model = model;
		}

		public string Model { get; }
	}
}
