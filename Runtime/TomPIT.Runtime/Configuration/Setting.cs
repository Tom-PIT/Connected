using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;

namespace TomPIT.Configuration
{
	internal class Setting : ISetting
	{
      [KeyProperty]
      [InvalidateEnvironment(EnvironmentSection.Designer)]
      [Required]
      [MaxLength(128)]
      [PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
      public string Name { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryData)]
      [MaxLength(1024)]
      public string Value { get; set; }

      public string Type { get; set; }
      public string PrimaryKey { get; set; }
      public string NameSpace { get; set; }
      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name)
            ? base.ToString()
            : Name;
      }
   }
}
