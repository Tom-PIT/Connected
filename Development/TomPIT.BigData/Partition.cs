using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Events;

namespace TomPIT.BigData
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class Partition : ComponentConfiguration, IPartitionConfiguration
	{
		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
