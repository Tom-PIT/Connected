using TomPIT.Annotations;
using TomPIT.Annotations.Models;

namespace TomPIT.Data
{
	public abstract class VersionedEntity : DataEntity
	{
		[PrimaryKey]
		[CacheKey]
		[ReturnValue]
		public virtual long Id { get; set; }

		[Version]
		public string Version { get; set; }
	}
}
