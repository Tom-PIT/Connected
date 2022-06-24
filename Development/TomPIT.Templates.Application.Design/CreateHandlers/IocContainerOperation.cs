using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.IoC;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class IocContainerOperation : ComponentCreateHandler<IIoCOperation>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.IocContainerOperation.txt";
	}
}