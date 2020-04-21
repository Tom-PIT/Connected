using System;

namespace TomPIT.ComponentModel.Apis
{
	[Flags]
	public enum ApiOperationVerbs
	{
		Get = 1,
		Post = 2,
		Delete = 4,
		Put = 8,
		Patch = 16,
		Head = 32,
		All = 127
	}

	public interface IOperationProtocolOptions : IApiProtocolOptions
	{
		ApiOperationVerbs RestVerbs { get; }
	}
}
