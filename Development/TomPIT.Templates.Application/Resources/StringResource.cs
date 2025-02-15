﻿using System.ComponentModel;
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
        public ListItems<IStringTranslation> Translations => _translations ??= new ListItems<IStringTranslation> { Parent = this };

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Key)
                ? base.ToString()
                : Key;
        }

        public void UpdateTranslation(int lcid, string value)
        {
            if (Translations.FirstOrDefault(f => f.Lcid == lcid) is StringTranslation existing)
            {
                if (string.IsNullOrWhiteSpace(value))
                    Translations.Remove(existing);
                else
                    existing.Value = value;
            }
            else
            {
                Translations.Add(new StringTranslation
                {
                    Lcid = lcid,
                    Value = value
                });
            }
        }
    }
}
