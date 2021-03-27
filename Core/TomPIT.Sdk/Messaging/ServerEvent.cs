using System;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;

namespace TomPIT.Messaging
{
	[Obsolete]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class ServerEvent : Text, IServerEvent
	{
		public override string FileName => $"{ToString()}.csx";
	}
}
