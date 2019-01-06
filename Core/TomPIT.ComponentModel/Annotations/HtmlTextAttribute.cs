using System;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HtmlTextAttribute : Attribute
	{
		public object Sanitize(IExecutionContext context, IElement element, PropertyInfo property, object existingValue, object proposedValue)
		{
			if (proposedValue == null)
				return proposedValue;

			if (property == null || element == null)
				return proposedValue;

			var htmlContentAttribute = property.FindAttribute<HtmlTextAttribute>();

			if (htmlContentAttribute != null)
				return SanitizeHtmlValue(context, element, property, existingValue, proposedValue);

			return proposedValue;
		}

		private object SanitizeHtmlValue(IExecutionContext context, IElement element, PropertyInfo property, object existingValue, object proposedValue)
		{
			if (property.PropertyType != typeof(string))
				return proposedValue;

			var sanitizer = new HtmlTextSanitizer(context, element);

			sanitizer.Sanitize(Convert.ToString(existingValue), Convert.ToString(proposedValue));

			return sanitizer.Result;
		}

	}
}
