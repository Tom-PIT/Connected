using System;
using TomPIT.Annotations.Design;
using TomPIT.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class AuthenticationToken : PrimaryKeyRecord, IAuthenticationToken
	{
      [KeyProperty]
      [Browsable(false)]
      public Guid Token { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      [Required]
      [MaxLength(128)]
      public string Key { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      public AuthenticationTokenClaim Claims { get; set; } = AuthenticationTokenClaim.None;

      [PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
      public AuthenticationTokenStatus Status { get; set; } = AuthenticationTokenStatus.Disabled;

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      [DateEditorFormat(DateEditorFormat.DateTime)]
      public DateTime ValidFrom { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      [DateEditorFormat(DateEditorFormat.DateTime)]
      public DateTime ValidTo { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      public TimeSpan StartTime { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      public TimeSpan EndTime { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      [PropertyEditor(PropertyEditorAttribute.Tag)]
      [MaxLength(2048)]
      [TagEditor(AllowCustomValues = true)]
      public string IpRestrictions { get; set; }

      [Browsable(false)]
      public Guid ResourceGroup { get; set; }

      [PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
      [PropertyEditor(PropertyEditorAttribute.Select)]
      [Items("TomPIT.Management.Items.UserItems, TomPIT.Management")]
      [Required]
      public Guid User { get; set; }

      [Required]
      [PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
      [MaxLength(128)]
      [InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
      public string Name { get; set; }

      [MaxLength(1024)]
      [PropertyCategory(PropertyCategoryAttribute.CategoryData)]
      [PropertyEditor(PropertyEditorAttribute.TextArea)]
      public string Description { get; set; }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name)
            ? base.ToString()
            : Name;
      }

      protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Key = GetString("key");
			Claims = GetValue("claims", AuthenticationTokenClaim.None);
			Status = GetValue("status", AuthenticationTokenStatus.Disabled);
			ValidFrom = GetDate("valid_from");
			ValidTo = GetDate("valid_to");
			StartTime = GetTimeSpan("start_time");
			EndTime = GetTimeSpan("end_time");
			IpRestrictions = GetString("ip_restrictions");
			ResourceGroup = GetGuid("resource_group_token");
			User = GetGuid("user_token");
			Name = GetString("name");
			Description = GetString("description");
		}
	}
}
