﻿using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public abstract class UIDependency : Dependency, IUIDependency
	{
	}
}