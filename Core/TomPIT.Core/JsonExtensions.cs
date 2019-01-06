using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using TomPIT.Exceptions;

namespace TomPIT
{
	public static class JsonExtensions
	{
		public static T Required<T>(this JObject instance, string propertyName)
		{
			if (!instance.ContainsKey(propertyName))
				throw new TomPITException(string.Format("{0} ({1}).", SR.ErrExpectedProperty, propertyName));

			try
			{
				return instance[propertyName].ToObject<T>();
			}
			catch
			{
				throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrInvalidPropertyType, propertyName, typeof(T)));
			}
		}

		public static T Optional<T>(this JObject instance, string propertyName, T defaultValue)
		{
			if (!instance.ContainsKey(propertyName))
				return defaultValue;

			try
			{
				return instance[propertyName].ToObject<T>();
			}
			catch
			{
				throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrInvalidPropertyType, propertyName, typeof(T)));
			}
		}

		public static T WithExisting<T>(this JObject instance, string propertyName, JObject existing)
		{
			if (!instance.ContainsKey(propertyName))
				return existing.Optional<T>(propertyName, default(T));

			try
			{
				return instance[propertyName].ToObject<T>();
			}
			catch
			{
				throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrInvalidPropertyType, propertyName, typeof(T)));
			}
		}

		public static bool IsEmpty(this JObject dataSource)
		{
			var arr = (JArray)dataSource["data"];

			return arr == null || arr.Count == 0;
		}

		public static JObject FirstResult(this JObject dataSource)
		{
			if (IsEmpty(dataSource))
				return new JObject();

			return ((JArray)dataSource["data"]).First as JObject;
		}

		public static JObject FirstResult(this JObject dataSource, string propertyName, object value)
		{
			var r = Where(dataSource, propertyName, value);

			if (IsEmpty(r))
				return new JObject();

			return FirstResult(r);
		}

		public static void Join(this JArray masterData, string masterKey, JArray detailsData, string detailsKey, params string[] properties)
		{
			if (masterData.Count == 0 || detailsData.Count == 0)
				return;

			foreach (var i in masterData)
			{
				if (!(i is JObject jo) || !jo.ContainsKey(masterKey))
					continue;

				var leftCriteria = jo[masterKey];

				foreach (var j in detailsData)
				{
					if (!(j is JObject jor) || !jo.ContainsKey(detailsKey))
						continue;

					var rightCriteria = jo[detailsKey];

					if (leftCriteria != rightCriteria)
						continue;

					foreach (var k in properties)
					{
						if (!jor.ContainsKey(k))
							continue;

						var val = jor[k];

						if (jo.ContainsKey(k))
							jo[k] = val;
						else
							jo.Add(k, val);
					}

					break;
				}
			}
		}

		public static void Join(this JArray masterData, string masterKey, JArray detailsData, string detailsKey, string detailsProperty)
		{
			if (masterData.Count == 0 || detailsData.Count == 0)
				return;

			foreach (var i in masterData)
			{
				var jo = i as JObject;

				if (!jo.ContainsKey(masterKey))
					continue;

				var masterValue = (string)jo[masterKey];

				foreach (var j in detailsData)
				{
					var jjo = j as JObject;

					if (!jjo.ContainsKey(detailsKey))
						continue;

					var detailValue = (string)j[detailsKey];

					if (string.Compare(masterValue, detailValue, true) == 0)
					{
						jo.Add(detailsProperty, j);
						break;
					}
				}
			}
		}

		public static void Join(this JObject masterData, string masterKey, JObject detailsData, string detailsKey, string detailsProperty)
		{
			Join(ToResults(masterData), masterKey, ToResults(detailsData), detailsKey, detailsProperty);
		}

		public static void Join(this JObject masterData, string masterKey, JArray detailsData, string detailsKey, string detailsProperty)
		{
			Join(ToResults(masterData), masterKey, detailsData, detailsKey, detailsProperty);
		}

		public static void Join(this JArray masterData, string masterKey, JObject detailsData, string detailsKey, string detailsProperty)
		{
			Join(masterData, masterKey, ToResults(detailsData), detailsKey, detailsProperty);
		}

		public static List<string> ToList(this JObject dataSource, string propertyName)
		{
			if (IsEmpty(dataSource))
				return new List<string>();

			return ToList(ToResults(dataSource), propertyName);
		}

		public static JArray ToArray(this JObject dataSource, string propertyName)
		{
			var r = new JArray();
			var results = ToResults(dataSource);

			foreach (var i in results)
			{
				if (!(i is JObject jo))
					continue;

				if (!jo.ContainsKey(propertyName))
					continue;

				var value = (string)jo[propertyName];

				if (string.IsNullOrWhiteSpace(value))
					continue;

				if (!r.Contains(value))
					r.Add(value);
			}

			return r;
		}

		public static List<string> ToList(this JObject dataSource)
		{
			var array = ToResults(dataSource);

			return ToList(array);
		}

		public static List<string> ToList(this JArray items)
		{
			var r = new List<string>();

			foreach (var i in items)
			{
				if (!(i is JObject jo))
					continue;

				if (jo.Count == 0)
					continue;

				if (!(jo.First is JProperty first))
					continue;

				var value = Types.Convert<string>(((JValue)first.Value).Value);

				if (string.IsNullOrWhiteSpace(value))
					continue;

				if (!r.Contains(value))
					r.Add(value);
			}

			return r;
		}

		public static List<string> ToList(this JArray items, string propertyName)
		{
			var r = new List<string>();

			foreach (var i in items)
			{
				if (!(i is JObject jo))
					continue;

				if (!jo.ContainsKey(propertyName))
					continue;

				var value = (string)jo[propertyName];

				if (string.IsNullOrWhiteSpace(value))
					continue;

				if (!r.Contains(value))
					r.Add(value);
			}

			return r;
		}

		public static JObject Lookup(this JObject dataSource, string propertyName, JArray values)
		{
			var list = new List<object>();

			foreach (var i in values)
				list.Add(i.Value<object>());

			return Lookup(dataSource, propertyName, list);
		}

		public static JObject Lookup(this JObject dataSource, string propertyName, List<object> values)
		{
			var vals = new List<string>();

			foreach (var i in values)
				vals.Add(Types.Convert<string>(i, CultureInfo.InvariantCulture));

			return Lookup(dataSource, propertyName, vals);
		}

		public static JObject Lookup(this JObject dataSource, string propertyName, List<string> values)
		{
			var arr = new JArray();
			var data = (JArray)dataSource["data"];

			foreach (var i in data)
			{
				if (!(i is JObject jo))
					continue;

				if (!jo.ContainsKey(propertyName))
					continue;

				var pv = i.Value<string>(propertyName);

				foreach (var j in values)
				{
					if (string.Compare(pv, j, true) == 0)
					{
						arr.Add(jo.DeepClone());
						break;
					}
				}
			}

			return new JObject
			{
				{"data", arr }
			};
		}

		public static JObject Where(this JObject dataSource, string propertyName, object value)
		{
			var val = Types.Convert<string>(value, CultureInfo.InvariantCulture);

			if (!(dataSource.SelectToken(string.Format("$.data[?(@.{0}=='{1}')]", propertyName, val)) is JObject r))
				return EmptyDataSource();

			return new JObject
			{
				{"data", new JArray
					{
						r
					}
				}
			};
		}

		public static JArray ToPropertyList(this JArray values, string propertyName)
		{
			var result = new JArray();

			foreach (var i in values)
				result.Add(new JObject{
					{"id", i.Value<string>()}
			});

			return result;
		}

		public static JArray ToResults(this JObject dataSource)
		{
			if (IsEmpty(dataSource))
				return new JArray();

			return (JArray)dataSource["data"];
		}

		private static JObject EmptyDataSource()
		{
			return new JObject
			{
				{"data", new JArray() }
			};
		}
	}
}
