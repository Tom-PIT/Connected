using System;

namespace TomPIT.Connectivity
{
	public interface IInstanceMetadataProvider
	{
		Guid InstanceId { get; }
	}
}
