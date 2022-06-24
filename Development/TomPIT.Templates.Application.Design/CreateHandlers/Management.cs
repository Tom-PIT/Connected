
using TomPIT.ComponentModel.Configuration;
using TomPIT.ComponentModel.Management;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Management : ComponentCreateHandler<IManagementConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Management.txt";
	}
}