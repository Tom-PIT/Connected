using System;
using TomPIT.Annotations.Design;
using TomPIT.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.Reflection;

namespace TomPIT.SysDb.Sql.Security
{
	internal class Role : PrimaryKeyRecord, IRole
	{
      [Browsable(false)]
      [KeyProperty]
      public Guid Token { get; set; }
      [PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
      [InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
      [MaxLength(128)]
      [Required]
      public string Name { get; set; }
      [Browsable(false)]
      public RoleBehavior Behavior { get; set; }
      [Browsable(false)]
      public RoleVisibility Visibility { get; set; }

      public override string ToString()
      {
         return string.IsNullOrWhiteSpace(Name)
            ? GetType().ShortName()
            : Name;
      }

      protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Name = GetString("name");
			Behavior = RoleBehavior.Explicit;
			Visibility = RoleVisibility.Visible;
		}
	}
}
