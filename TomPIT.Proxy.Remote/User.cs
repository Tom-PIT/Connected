using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class User : IUser
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[MaxLength(128)]
		public string FirstName { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[MaxLength(128)]
		public string LastName { get; set; }

		[Browsable(false)]
		public string Url { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		public string Email { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public UserStatus Status { get; set; }

		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }

		[Browsable(false)]
		public Guid AuthenticationToken { get; set; }

		[Items("TomPIT.Design.Items.LanguageItems, TomPIT.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		public Guid Language { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[MaxLength(1024)]
		public string Description { get; set; }

		[ReadOnly(true)]
		[PropertyEditor(PropertyEditorAttribute.Label)]
		[DisplayFormat(DataFormatString = "{0:g}")]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public DateTime LastLogin { get; set; }

		[Items("TomPIT.Design.Items.TimezoneItems, TomPIT.Design")]
		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public string TimeZone { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryCollaboration)]
		public bool NotificationEnabled { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[MaxLength(128)]
		public string LoginName { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[MaxLength(128)]
		public string Pin { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryCollaboration)]
		[MaxLength(48)]
		public string Phone { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryCollaboration)]
		[MaxLength(48)]
		public string Mobile { get; set; }

		[Browsable(false)]
		public Guid Avatar { get; set; }
		[Browsable(false)]
		public DateTime PasswordChange { get; set; }
		[Browsable(false)]
		public bool HasPassword { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[MaxLength(128)]
		public string SecurityCode { get; set; }

		public override string ToString()
		{
			return this.DisplayName();
		}
	}
}
