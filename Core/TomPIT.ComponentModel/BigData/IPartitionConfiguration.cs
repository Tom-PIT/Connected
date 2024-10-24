﻿using TomPIT.Collections;

namespace TomPIT.ComponentModel.BigData
{
	public enum SchemaSynchronizationMode
	{
		Manual = 1,
		Auto = 2
	}
	public interface IPartitionConfiguration : IConfiguration, IText, INamespaceElement
	{
		SchemaSynchronizationMode SchemaSynchronization { get; }

		ListItems<IBigDataQuery> Queries { get; }
	}
}
