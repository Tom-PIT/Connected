using System;

namespace TomPIT.Environment
{
	public enum InstanceStatus
	{
		Disabled = 0,
		Enabled = 1,
	}

	public enum InstanceType
	{
		Unknown = 0,
		Management = 1,
		Development = 2,
		Application = 3,
		Worker = 4,
		Cdn = 5,
		IoT = 6,
		BigData = 7,
		Search = 8,
		Rest = 9
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
		InstanceType Type { get; }
		Guid Token { get; }
		InstanceVerbs Verbs { get; }
		string ReverseProxyUrl { get; }
	}
}
