using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.MicroServices.BigData.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.BigData
{
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[DomDesigner(DesignUtils.BigDataPartitionDesigner, Mode = EnvironmentMode.Runtime)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Partition : SourceCodeConfiguration, IPartitionConfiguration
	{
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(SchemaSynchronizationMode.Manual)]
		public SchemaSynchronizationMode SchemaSynchronization { get; set; } = SchemaSynchronizationMode.Manual;
	}
}
