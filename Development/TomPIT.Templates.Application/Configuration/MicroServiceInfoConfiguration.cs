﻿using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;

namespace TomPIT.MicroServices.Configuration
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.MicroServiceInfo, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class MicroServiceInfoConfiguration : TextConfiguration, IMicroServiceInfoConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
