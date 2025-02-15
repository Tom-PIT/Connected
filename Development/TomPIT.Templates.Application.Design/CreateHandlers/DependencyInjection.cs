﻿using System.Text.RegularExpressions;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.IoC;
using TomPIT.Development.Handlers;

namespace TomPIT.MicroServices.Design.CreateHandlers
{
	internal class DependencyInjection : ComponentCreateHandler<IDependency>
	{
		protected override string Template => "TomPIT.MicroServices.Design.CreateHandlers.Templates.DependencyInjection.txt";
	}
}
