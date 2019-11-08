using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.BigData;
using TomPIT.Runtime;

namespace TomPIT.Management.BigData
{
	internal class Node : INode
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[MaxLength(128)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		[Required]
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[MaxLength(256)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string ConnectionString { get; set; }
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[MaxLength(256)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string AdminConnectionString { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public NodeStatus Status { get; set; }
		[Browsable(false)]
		public long Size { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
