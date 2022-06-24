
using TomPIT.ComponentModel.Configuration;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Settings : ComponentCreateHandler<ISettingsConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Settings.txt";
	}
}