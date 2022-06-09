
using TomPIT.ComponentModel.Navigation;
using TomPIT.Development.Handlers;
using TomPIT.Distributed;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class SiteMap : ComponentCreateHandler<ISiteMapConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.SiteMap.txt";
	}
}