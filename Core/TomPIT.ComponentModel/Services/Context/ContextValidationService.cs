using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;

namespace TomPIT.Services.Context
{
	internal class ContextValidationService : ContextClient, IContextValidationService
	{
		public ContextValidationService(IExecutionContext context) : base(context)
		{
		}

		public void Email(string attribute, string value)
		{
			try
			{
				new MailAddress(value);
			}
			catch (FormatException)
			{
				throw new RuntimeException(SR.SourceValidation, SR.ValEmailFormat);
			}
		}

		public void Exists(string attribute, object value, List<object> values)
		{
			if (values == null || values.Count == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValNotExists, value, attribute));

			foreach (var i in values)
			{
				if (Types.Compare(value, i))
					return;
			}

			throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValNotExists, value, attribute));
		}

		public void Exists(string attribute, object value, List<string> values)
		{
			if (values == null || values.Count == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValNotExists, value, attribute));

			foreach (var i in values)
			{
				if (Types.Compare(value, i))
					return;
			}

			throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValNotExists, value, attribute));
		}

		public void MaxLength(string attribute, string value, int length)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			if (value.Length > length)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValMaxLength, length, attribute));
		}

		public void MaxValue(string attribute, double value, double maxValue)
		{
			if (value > maxValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(string.Format("{0} ({1})", string.Format(SR.ValMaxValue, maxValue), attribute)));
		}

		public void MaxValue(string attribute, DateTime value, DateTime maxValue)
		{
			if (value > maxValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(string.Format("{0} ({1})", string.Format(SR.ValMaxValue, maxValue), attribute)));
		}

		public void MinLength(string attribute, string value, int length)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			if (value.Length < length)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValMinLength, length, attribute));
		}

		public void MinValue(string attribute, double value, double minValue)
		{
			if (value < minValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(string.Format("{0} ({1})", string.Format(SR.ValMinValue, minValue), attribute)));
		}

		public void MinValue(string attribute, DateTime value, DateTime minValue)
		{
			if (value < minValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(string.Format("{0} ({1})", string.Format(SR.ValMinValue, minValue), attribute)));
		}

		public void Range(string attribute, double value, double minValue, double maxValue)
		{
			if (value < minValue || value > maxValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRange, minValue, maxValue, attribute));
		}

		public void Range(string attribute, DateTime value, DateTime minValue, DateTime maxValue)
		{
			if (value < minValue || value > maxValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRange, minValue, maxValue, attribute));
		}

		public void Required(string attribute, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, int value)
		{
			if (value == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, long value)
		{
			if (value == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, float value)
		{
			if (value == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, double value)
		{
			if (value == 0)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, DateTime value)
		{
			if (value == DateTime.MinValue)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Required(string attribute, Guid value)
		{
			if (value == Guid.Empty)
				throw new RuntimeException(SR.SourceValidation, string.Format(SR.ValRequired, attribute));
		}

		public void Unique(string attribute, object value, string keyProperty, object keyValue, JObject existing)
		{
			if (existing == null)
				return;

			var tokens = existing.SelectTokens(string.Format("$.data[?(@.{0}=='{1}')]", attribute, value));

			foreach (var i in tokens)
			{
				if (!(i is JObject jo))
					continue;

				var val = jo[keyProperty];

				var sl = (string)val;
				var sr = Types.Convert<string>(keyValue, CultureInfo.InvariantCulture);

				if (string.Compare(sl, sr, true) == 0)
					continue;

				throw new RuntimeException(SR.SourceValidation, string.Format("{0}. ({1})", SR.ValExists, attribute));
			}
		}
	}
}
