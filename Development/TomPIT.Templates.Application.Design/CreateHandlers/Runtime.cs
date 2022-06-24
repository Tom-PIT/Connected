
using TomPIT.ComponentModel.Navigation;
using TomPIT.ComponentModel.Runtime;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Runtime : ComponentCreateHandler<IRuntimeConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Runtime.txt";
	}
}