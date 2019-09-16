using System;

namespace TomPIT.Annotations.Design
{
	public enum CollectionRuntimeMerge
	{
		Synchronize = 1,
		Override = 2,
		Append = 3
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class CollectionRuntimeMergeAttribute : Attribute
	{
		public CollectionRuntimeMergeAttribute(CollectionRuntimeMerge mode)
		{
			Mode = mode;
		}

		public CollectionRuntimeMerge Mode { get; }
	}
}
