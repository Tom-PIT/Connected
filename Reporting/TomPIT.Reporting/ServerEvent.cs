using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Reporting
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax("csharp")]
	internal class ServerEvent : Text, IServerEvent
	{
	}
}
