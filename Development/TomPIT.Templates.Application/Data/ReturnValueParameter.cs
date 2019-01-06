using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("ReturnValue", "Name")]
	[SuppressProperties("IsNullable,NullMapping, SupportsTimeZone")]
	public class ReturnValueParameter : Parameter, IReturnValueParameter
	{
	}
}
