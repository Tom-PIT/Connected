using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareValidationService
	{
		void MaxLength(string memberName, string value, int length);
		void MinLength(string memberName, string value, int length);

		void MinValue(string memberName, double value, double minValue);
		void MaxValue(string memberName, double value, double maxValue);
		void Range(string memberName, double value, double minValue, double maxValue);

		void MinValue(string memberName, DateTime value, DateTime minValue);
		void MaxValue(string memberName, DateTime value, DateTime maxValue);
		void Range(string memberName, DateTime value, DateTime minValue, DateTime maxValue);

		void Required(string memberName, string value);
		void Required(string memberName, int value);
		void Required(string memberName, long value);
		void Required(string memberName, float value);
		void Required(string memberName, double value);
		void Required(string memberName, DateTime value);
		void Required(string memberName, Guid value);

		void Unique(string memberName, object value, string keyProperty, object keyValue, JObject existing);
		void Unique(string memberName, object value, string keyProperty, object keyValue, string propertyName, List<object> existing);
		void Exists(string memberName, object value, List<object> values);
		void Exists(string memberName, object value, List<string> values);

		void Email(string memberName, string value);
	}
}
