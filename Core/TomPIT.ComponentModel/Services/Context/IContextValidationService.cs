using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TomPIT.Services.Context
{
	public interface IContextValidationService
	{
		void MaxLength(string attribute, string value, int length);
		void MinLength(string attribute, string value, int length);

		void MinValue(string attribute, double value, double minValue);
		void MaxValue(string attribute, double value, double maxValue);
		void Range(string attribute, double value, double minValue, double maxValue);

		void MinValue(string attribute, DateTime value, DateTime minValue);
		void MaxValue(string attribute, DateTime value, DateTime maxValue);
		void Range(string attribute, DateTime value, DateTime minValue, DateTime maxValue);

		void Required(string attribute, string value);
		void Required(string attribute, int value);
		void Required(string attribute, long value);
		void Required(string attribute, float value);
		void Required(string attribute, double value);
		void Required(string attribute, DateTime value);
		void Required(string attribute, Guid value);

		void Unique(string attribute, object value, string keyProperty, object keyValue, JObject existing);
		void Exists(string attribute, object value, List<object> values);
		void Exists(string attribute, object value, List<string> values);
	}
}
