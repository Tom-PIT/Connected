using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Scripting
{
	[Create(DesignUtils.Class)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler(DesignUtils.ScriptCreateHandler)]
	[DomElement(DesignUtils.ScriptElement)]
	public class Script : ComponentConfiguration, IScriptConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Internal)]
		[InvalidateEnvironment(EnvironmentSection.Explorer)]
		public ElementScope Scope { get; set; } = ElementScope.Internal;

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}