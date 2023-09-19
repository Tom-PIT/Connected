using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.TupleExtensions;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT
{
    public class StringProcessorHelper : HelperBase
    {
        public StringProcessorHelper(IHtmlHelper helper) : base(helper)
        {
        }

        public IHtmlContent Render(string value, params (string property, object value)[][] replacements)
        {
            return new HtmlString(StringProcessor.Process(value, replacements));
        }

        public IHtmlContent Render(string value, params object[] replacements)
        {
            var convertedReplacements = replacements.Select(replacement => {
                var props = replacement.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |System.Reflection.BindingFlags.GetProperty);
                return props.Select(e => (e.Name, e.GetValue(replacement))).ToArray();
            });

            return new HtmlString(StringProcessor.Process(value, convertedReplacements.ToArray()));
        }
    }
}
