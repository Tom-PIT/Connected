
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;

namespace TomPIT.Application.Apis
{
	internal class OperationProtocolOptions : ApiProtocolOptions, IOperationProtocolOptions
	{
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public ApiOperationVerbs RestVerbs { get; set; } = ApiOperationVerbs.All;
	}
}
