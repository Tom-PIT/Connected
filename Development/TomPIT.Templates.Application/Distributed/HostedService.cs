using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.MicroServices.Distributed
{
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.HostedService, TomPIT.MicroServices.Design")]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[ClassRequired]
	public class HostedService : TextConfiguration, IHostedServiceConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
