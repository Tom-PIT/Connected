using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Schema
{
	public abstract class SchemaField : ConfigurationElement, ISchemaField
	{
		[InvalidateEnvironment(EnvironmentSection.Designer | EnvironmentSection.Explorer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool IsKey { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool Index { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
