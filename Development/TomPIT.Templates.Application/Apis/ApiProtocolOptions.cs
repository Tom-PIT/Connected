using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Application.Apis
{
	internal class ApiProtocolOptions : Element, IApiProtocolOptions
	{
		public bool Rest { get; set; }
	}
}
