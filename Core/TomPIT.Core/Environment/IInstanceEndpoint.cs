using System;

namespace TomPIT.Environment
{
	public enum InstanceStatus
	{
		Disabled = 0,
		Enabled = 1,
	}

	[Flags]
	public enum InstanceFeatures
	{
		Unknown = 0,
		Management = 1,
		Development = 2,
		Application = 4,
		Worker = 8,
		Cdn = 16,
		IoT = 32,
		BigData = 64,
		Search = 128,
		Rest = 256,
		Sys = 512
	}

	[Flags]
	public enum InstanceVerbs
	{
		Get = 1,
		Post = 2,
		All = 3
	}

	public interface IInstanceEndpoint
	{
		string Url { get; }
		InstanceStatus Status { get; }
		string Name { get; }
		InstanceFeatures Features { get; }
		Guid Token { get; }
		InstanceVerbs Verbs { get; }
		string ReverseProxyUrl { get; }
	}
}
