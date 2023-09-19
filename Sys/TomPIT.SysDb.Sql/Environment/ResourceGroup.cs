using System;
using TomPIT.Annotations.Design;
using TomPIT.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
   internal class ResourceGroup : PrimaryKeyRecord, IServerResourceGroup
   {
      [PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
      [InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
      [MaxLength(128)]
      [Required]
      public string Name { get; set; }
      [Browsable(false)]
      [KeyProperty]
      public Guid Token { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryData)]
      [PropertyEditor(PropertyEditorAttribute.Select)]
      //[Items(ManagementItems.StorageProvider)]
      public Guid StorageProvider { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryData)]
      public string ConnectionString { get; set; }

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
         Token = GetGuid("token");
         StorageProvider = GetGuid("storage_provider");
         ConnectionString = GetString("connection_string");
      }
   }
}
