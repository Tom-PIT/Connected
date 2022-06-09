using TomPIT.ComponentModel.Messaging;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
    internal class EventBinding : ComponentCreateHandler<IEventBinding>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.EventBinding.txt";
	}
}
