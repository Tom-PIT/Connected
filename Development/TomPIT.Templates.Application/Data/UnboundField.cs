using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("UnboundField", nameof(Name))]
	public class UnboundField : DataField, IUnboundField
	{
	}
}
