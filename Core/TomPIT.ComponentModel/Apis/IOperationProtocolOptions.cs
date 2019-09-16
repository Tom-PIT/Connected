using System;

namespace TomPIT.ComponentModel.Apis
{
	[Flags]
	public enum ApiOperationVerbs
	{
		Get = 1,
		Post = 2,
		All = 127
	}

	public interface IOperationProtocolOptions : IApiProtocolOptions
	{
		ApiOperationVerbs RestVerbs { get; }
	}
}
