using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

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
			else if (value is DateTime)
				return Date((DateTime)value);
			else if (value is Guid)
				return String(value);
			else if (value is bool)
				return Bool((bool)value);
			else if (Types.IsNumericType(value.GetType()))
				return Number(value, mapNull);

			return Html.Raw(value);
		}
		public IHtmlContent Value(object value)
		{
			return Value(value, false);
		}

		public IHtmlContent Number(object value, bool mapNull, int decimalPlaces)
		{
			if (value == null)
				return Html.Raw("null") as HtmlString;

			var number = Convert.ToDecimal(value);

			if(mapNull && number == decimal.Zero)
				return Html.Raw("null") as HtmlString;

			if (decimalPlaces == -1)
				return Html.Raw(string.Format("{0}", number.ToString())) as HtmlString;
			else
				return Html.Raw(string.Format("{0}", number.ToString($"n{decimalPlaces}"))) as HtmlString;
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

			return Html.Raw(string.Format("'{0}'", value.ToString())) as HtmlString;
		}

		public IHtmlContent Bool(bool value)
		{
			if (value)
				return Html.Raw("true") as HtmlString;
			else
				return Html.Raw("false") as HtmlString;
		}

		public IHtmlContent Date(DateTime date)
		{
			if (date == DateTime.MinValue)
				return Html.Raw("null") as HtmlString;

			return Html.Raw(string.Format("new Date({0}, {1}, {2}, {3}, {4}, {5})", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second));
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
	}
}
