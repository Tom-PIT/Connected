using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class AuthenticationToken : IAuthenticationToken
	{
		[KeyProperty]
		[Browsable(false)]
		public Guid Token { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[Required]
		[MaxLength(128)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public string Key { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public AuthenticationTokenClaim Claims { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public AuthenticationTokenStatus Status { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[DateEditorFormat(DateEditorFormat.DateTime)]
		public DateTime ValidFrom { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[DateEditorFormat(DateEditorFormat.DateTime)]
		public DateTime ValidTo { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public TimeSpan StartTime { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public TimeSpan EndTime { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyEditor(PropertyEditorAttribute.Tag)]
		[MaxLength(2048)]
		[TagEditor(AllowCustomValues = true)]
		public string IpRestrictions { get; set; }

		[Browsable(false)]
		public Guid ResourceGroup { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Management.Items.UserItems, TomPIT.Management")]
		[Required]
		public Guid User { get; set; }

		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[MaxLength(128)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public string Name { get; set; }

		[MaxLength(1024)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public string Description { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
