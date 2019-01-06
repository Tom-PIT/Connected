using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application
{
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Design")]
	[Syntax("csharp")]
	internal class ServerEvent : Text, IServerEvent
	{
	}
}
