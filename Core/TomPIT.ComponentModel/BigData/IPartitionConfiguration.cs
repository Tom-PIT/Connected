﻿namespace TomPIT.ComponentModel.BigData
{
	public enum SchemaSynchronizationMode
	{
		Manual = 1,
		Auto = 2
	}
	public interface IPartitionConfiguration : IConfiguration, IText
	{
		SchemaSynchronizationMode SchemaSynchronization { get; }
	}
}