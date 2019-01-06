
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Application.Apis
{
	internal class OperationProtocolOptions : ApiProtocolOptions, IOperationProtocolOptions
	{
		public ApiOperationVerbs RestVerbs { get; set; } = ApiOperationVerbs.All;
	}
}
