using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
   internal class MicroService : PrimaryKeyRecord, IMicroService
   {
      [MaxLength(128)]
      [Required]
      public string Name { get; set; }
      [Browsable(false)]
      public string Url { get; set; }
      [Browsable(false)]
      public Guid Token { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
      public MicroServiceStatus Status { get; set; }
      [Browsable(false)]
      public Guid ResourceGroup { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryData)]
      [PropertyEditor(PropertyEditorAttribute.Select)]
      [Items("TomPIT.Management.Items.MicroServiceTemplatesItems, TomPIT.Management")]
      public Guid Template { get; set; }
      [Browsable(false)]
      public Guid Package { get; set; }
      [Browsable(false)]
      public UpdateStatus UpdateStatus { get; set; }
      [Browsable(false)]
      public CommitStatus CommitStatus { get; set; }
      [Editable(false)]
      public string Version { get; set; }
      [Browsable(false)]
      public Guid Plan { get; set; }

      protected override void OnCreate()
      {
         base.OnCreate();

         Name = GetString("name");
         Url = GetString("url");
         Token = GetGuid("token");
         Status = GetValue("status", MicroServiceStatus.Development);
         ResourceGroup = GetGuid("resource_token");
         Template = GetGuid("template");
         Package = GetGuid("package");
         UpdateStatus = GetValue("update_status", UpdateStatus.UpToDate);
         CommitStatus = GetValue("commit_status", CommitStatus.Synchronized);
         Version = GetString("version");
         Plan = GetGuid("plan");
      }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name)
             ? base.ToString()
             : Name;
      }
   }
}