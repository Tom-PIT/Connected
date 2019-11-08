using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Apis;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Apis
{
	internal class OperationProtocolOptions : ApiProtocolOptions, IOperationProtocolOptions
	{
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public ApiOperationVerbs RestVerbs { get; set; } = ApiOperationVerbs.All;
	}
}
