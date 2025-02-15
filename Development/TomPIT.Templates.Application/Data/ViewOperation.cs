﻿using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Data;

namespace TomPIT.MicroServices.Data
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Sql)]
	public class ViewOperation : ModelOperation, IViewOperation
	{
		[Browsable(false)]
		public override string FileName => $"{ToString()}.sql";
	}
}
