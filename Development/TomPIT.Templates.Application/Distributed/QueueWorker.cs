using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Distributed
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class QueueWorker : SourceCodeElement, IQueueWorker
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}
	}
}
