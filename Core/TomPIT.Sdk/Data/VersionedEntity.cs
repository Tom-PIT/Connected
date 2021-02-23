using System;
using TomPIT.Annotations;
using TomPIT.Annotations.Models;

namespace TomPIT.Data
{
	[Obsolete("Please use VersionedEntity<T> instead.")]
	public abstract class VersionedEntity : VersionedEntity<long>
	{
	}

	public abstract class VersionedEntity<T> : DataEntity
	{
		[PrimaryKey]
		[CacheKey]
		[ReturnValue]
		public virtual T Id { get; set; }

		[Version]
		public string Version { get; set; }
	}
}
