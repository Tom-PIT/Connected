using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT
{
	public class JavascriptHelper : HelperBase
	{
		public JavascriptHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public IHtmlContent Value(object value, bool mapNull)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			if (value is string || value is char)
				return String(value);
			else if (value is DateTime time)
				return Date(time);
			else if (value is Guid)
				return Guid(value);
			else if (value is bool)
				return Bool((bool)value);
			else if (value.GetType().IsNumericType())
				return Number(value, mapNull);

			return Html.Raw(value);
		}
		public IHtmlContent Value(object value)
		{
			return Value(value, false);
		}

		public IHtmlContent Guid(object value)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			if (Types.TryConvert(value, out Guid converted))
			{
				if (converted == System.Guid.Empty)
					return Html.Raw("null") as HtmlString;
				else
					return Html.Raw($"'{converted}'") as HtmlString;
			}
			else
				return Html.Raw("null") as HtmlString;
		}

		public IHtmlContent Number(object value, bool mapNull, int decimalPlaces)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			var number = Convert.ToDecimal(value);

			if (mapNull && number == decimal.Zero)
				return Html.Raw("null") as HtmlString;

			if (decimalPlaces == -1)
				return Html.Raw(number.ToString(CultureInfo.InvariantCulture)) as HtmlString;
			else
				return Html.Raw($"{number.ToString($"n{decimalPlaces}", CultureInfo.InvariantCulture)}") as HtmlString;
		}

		public IHtmlContent Number(object value, bool mapNull)
		{
			return Number(value, mapNull, -1);
		}

		public IHtmlContent Number(object value)
		{
			return Number(value, false, -1);
		}

		public IHtmlContent String(object value)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			return Html.Raw(string.Format("'{0}'", HttpUtility.JavaScriptStringEncode(value.ToString()))) as HtmlString;
		}

		public IHtmlContent Bool(object value)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			if (!bool.TryParse(value.ToString(), out bool result))
				return Html.Raw("null") as HtmlString;

			return Bool(result);
		}

		public IHtmlContent Bool(bool value)
		{
			if (value)
				return Html.Raw("true") as HtmlString;
			else
				return Html.Raw("false") as HtmlString;
		}

		public IHtmlContent Date(object value)
		{
			if(value == null)
				return Html.Raw("null") as HtmlString;

			if(!DateTime.TryParse(value.ToString(), out DateTime result))
				return Html.Raw("null") as HtmlString;

			return Date(result);
		}
		public IHtmlContent Date(DateTime date)
		{
			if (date == DateTime.MinValue)
				return Html.Raw("null") as HtmlString;

			return Html.Raw($"new Date(Date.UTC({date.Year}, {date.Month - 1}, {date.Day}, {date.Hour}, {date.Minute}, {date.Second}))");
		}

		public IHtmlContent Array(IEnumerable<string> items)
		{
			var sb = new StringBuilder();

			sb.Append("[");

			foreach (var i in items)
				sb.AppendFormat("'{0}',", i);

			sb.Append("]");

			return Html.Raw(sb.ToString());
		}

		public IHtmlContent Object(object value)
		{
			if (value == null)
				return Html.Raw("{}");

			return Html.Raw(Serializer.Serialize(value));
		}
		public IHtmlContent BundlePath([CIP(CIP.BundleProvider)]string name)
		{
			var model = Html.ViewData.Model as IMiddlewareContext;

			return Html.Raw($"{model.Services.Routing.RootUrl}/sys/bundles/{name}".ToLowerInvariant());
		}
	}
}
