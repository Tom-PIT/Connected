using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Apis
{
	[DomElement(DesignUtils.ApiOperationElement)]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[Create("Operation", nameof(Name))]
	[ClassRequired]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.ApiOperation, TomPIT.MicroServices.Design")]
	public class Operation : ConfigurationElement, IApiOperation
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		[InvalidateEnvironment(EnvironmentSection.Explorer)]
		public ElementScope Scope { get; set; } = ElementScope.Public;

		[Browsable(false)]
		public Guid TextBlob { get; set; }
		[Browsable(false)]
		public string FileName => $"{ToString()}.csx";
	}
}
