using System;

namespace TomPIT.Annotations
{
	public sealed class DependencyAttribute : Attribute
	{
		public DependencyAttribute(Type model)
		{
			Model = model;
		}

		public DependencyAttribute(Type model, string property)
		{
			Model = model;
			Property = property;
		}

		public Type Model { get; }
		public string Property { get; }
	}
}
