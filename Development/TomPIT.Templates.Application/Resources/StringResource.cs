using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.String, nameof(Key))]
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

		[Browsable(false)]
		[CollectionRuntimeMerge(CollectionRuntimeMerge.Override)]
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

		public void UpdateTranslation(int lcid, string value, bool auditChange)
		{
			if (Translations.FirstOrDefault(f => f.Lcid == lcid) is StringTranslation existing)
			{
				existing.Value = value;
				existing.Changed = auditChange;
			}
			else
			{
				Translations.Add(new StringTranslation
				{
					Lcid = lcid,
					Value = value,
					Changed = auditChange
				});
			}
		}
	}
}
