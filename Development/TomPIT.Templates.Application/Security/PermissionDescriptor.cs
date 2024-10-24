﻿using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Security;

namespace TomPIT.MicroServices.Security
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class PermissionDescriptor : TextConfiguration, IPermissionDescriptorConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
