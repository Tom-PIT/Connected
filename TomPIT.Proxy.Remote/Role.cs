using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class Role : IRole
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
	}
}
