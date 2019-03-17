using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("String", nameof(Key))]
	public class StringResource : ConfigurationElement, IStringResource
	{
		private ListItems<IStringTranslation> _translations = null;

		[Required]
		[InvalidateEnvironment(EnvironmentSection.Designer | EnvironmentSection.Explorer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Key { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		public string DefaultValue { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool IsLocalizable { get; set; } = true;

		[EnvironmentVisibility(Services.EnvironmentMode.Runtime)]
		public ListItems<IStringTranslation> Translations
		{
			get
			{
				if (_translations == null)
					_translations = new ListItems<IStringTranslation> { Parent = this };

				return _translations;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Key)
				? base.ToString()
				: Key;
		}
	}
}
