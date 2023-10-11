using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DesignUtils.ScheduleDesigner, Mode = EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.HostedWorker, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class HostedWorker : TextConfiguration, IHostedWorkerConfiguration
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
