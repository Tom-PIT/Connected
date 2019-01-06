using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public class AttributesHelper : HelperBase
	{
		public AttributesHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public HtmlString Checked(bool condition)
		{
			if (!condition)
				return HtmlString.Empty;

			return new HtmlString("checked");
		}

		public HtmlString Selected(bool condition)
		{
			if (!condition)
				return HtmlString.Empty;

			return new HtmlString("selected");
		}

		public HtmlString Disabled(bool condition)
		{
			if (!condition)
				return HtmlString.Empty;

			return new HtmlString("disabled");
		}

		public HtmlString Class(string value)
		{
			return Attribute("class", value);
		}

		public HtmlString Attribute(string attribute, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			return new HtmlString(Html.Raw(string.Format("{0}=\"{1}\"", attribute, value)).ToString());
		}

		public HtmlString Attribute(string attribute, string value, bool condition)
		{
			if (!condition)
				return null;

			return Attribute(attribute, value);
		}

	}
}
