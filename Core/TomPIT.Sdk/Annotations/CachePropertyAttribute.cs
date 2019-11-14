using System;

namespace TomPIT.Annotations
{
	public enum CachePropertyVisibility
	{
		Visible = 1,
		Hidden = 2
	}

	public enum CachePropertyStorage
	{
		Optimized = 1,
		Raw = 2
	}

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class CachePropertyAttribute : Attribute
	{
		public CachePropertyAttribute()
		{

		}

		public CachePropertyAttribute(CachePropertyVisibility visibility)
		{
			Visibility = visibility;
		}

		public CachePropertyAttribute(CachePropertyVisibility visibility, CachePropertyStorage storage)
		{
			Visibility = visibility;
			Storage = storage;
		}

		public CachePropertyVisibility Visibility { get; } = CachePropertyVisibility.Visible;
		public CachePropertyStorage Storage { get; } = CachePropertyStorage.Optimized;
	}
}
