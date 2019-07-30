using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;

namespace TomPIT.BigData
{
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[DomDesigner("TomPIT.Management.Designers.BigDataPartitionDesigner, TomPIT.Management", Mode = EnvironmentMode.Runtime)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Partition : ComponentConfiguration, IPartitionConfiguration
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(SchemaSynchronizationMode.Manual)]
		public SchemaSynchronizationMode SchemaSynchronization { get; set; } = SchemaSynchronizationMode.Manual;
	}
}
