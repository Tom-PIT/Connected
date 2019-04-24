using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
    [Create("Class")]
    [DomDesigner(DomDesignerAttribute.TextDesigner)]
    [DomElement("TomPIT.Application.Design.Dom.ViewElement, TomPIT.Application.Design")]
    [Syntax(SyntaxAttribute.CSharp)]
    public class Script : ComponentConfiguration, IScript
    {
        public const string ComponentCategory = "Script";

        [PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
        [DefaultValue(ElementScope.Internal)]
        public ElementScope Scope { get; set; } = ElementScope.Internal;

        [Browsable(false)]
        public Guid TextBlob { get; set; }
    }
}
