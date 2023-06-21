using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using TomPIT.Annotations.Design;
using TomPIT.Annotations;
using TomPIT.Configuration;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Configuration
{
   internal class Setting : PrimaryKeyRecord, ISetting
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

      protected override void OnCreate()
      {
         base.OnCreate();

         Name = GetString("name");
         Value = GetString("value");
         Type = GetString("type");
         PrimaryKey = GetString("primary_key");
         NameSpace = GetString("namespace");
      }
   }
}
