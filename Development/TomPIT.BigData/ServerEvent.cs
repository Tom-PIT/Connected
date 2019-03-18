using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.BigData
{
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax("csharp")]
	internal class ServerEvent : Text, IServerEvent
	{
	}
}
