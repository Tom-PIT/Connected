﻿using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.IoC;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.IoC
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class MasterDependency : UIDependency, IMasterDependency
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(DesignUtils.MasterItems)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Master { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(MasterDependencyKind.Server)]
		public MasterDependencyKind Kind { get; set; } = MasterDependencyKind.Server;
	}
}