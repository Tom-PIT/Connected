using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;

namespace TomPIT.ComponentModel.BigData
{
	public enum SchemaSynchronizationMode
	{
		Manual = 1,
		Auto = 2
	}
	public interface IPartitionConfiguration : IConfiguration, ISourceCode
	{
		SchemaSynchronizationMode SchemaSynchronization { get; }
	}
}
