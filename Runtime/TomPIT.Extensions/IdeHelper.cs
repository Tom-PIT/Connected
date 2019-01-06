using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;

namespace TomPIT
{
	public class IdeHelper : HelperBase
	{
		public IdeHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public HtmlString TypeSignature(Type type)
		{
			if (!type.IsGenericType)
				return new HtmlString(type.Name);

			var args = type.GenericTypeArguments;

			var sb = new StringBuilder();

			var tn = type.Name.Split('`');

			sb.AppendFormat("{0} <", tn);

			foreach (var i in args)
				sb.AppendFormat("{0}, ", TypeSignature(i));

			return Html.Raw(Html.Encode(string.Format("{0}>", sb.ToString().Trim().TrimEnd(',')))) as HtmlString;
		}
	}
}
