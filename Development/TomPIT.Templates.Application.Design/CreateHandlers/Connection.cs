using TomPIT.ComponentModel.Data;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class Connection : ComponentCreateHandler<IConnectionConfiguration>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.Connection.txt";
	}
}
