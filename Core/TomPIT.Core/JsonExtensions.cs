using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TomPIT
{
	public static class JsonExtensions
	{
		public static T Required<T>(this JObject instance, string propertyName)
		{
			var property = instance.Property(propertyName, StringComparison.OrdinalIgnoreCase);

			if (property == null)
				throw new TomPITException(string.Format("{0} ({1}).", SR.ErrExpectedProperty, propertyName));

			try
			{
				object proposed = property.Value;

				if (property.Value is JValue val)
				{
					if (val.Value == null)
						throw new TomPITException(string.Format("{0} ({1}).", SR.ErrExpectedPropertyValue, propertyName));

					proposed = val.Value;
				}

				T r;

				if (typeof(T).IsSubclassOf(typeof(JToken)))
				{
					if (!Types.TryConvert(proposed, out r))
						throw new TomPITException(string.Format("{0} ({1}).", SR.ErrExpectedPropertyValue, propertyName));
				}
				else
					return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(proposed));

				if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(r as string))
					throw new TomPITException(string.Format("{0} ({1}).", SR.ErrExpectedPropertyValue, propertyName));

				return r;
			}
			catch (Exception ex)
			{
				if (ex is TomPITException)
					throw ex;

				throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrInvalidPropertyType, propertyName, typeof(T)));
			}
		}

		public static T Optional<T>(this JObject instance, string propertyName, T defaultValue)
		{
			var property = instance.Property(propertyName, StringComparison.OrdinalIgnoreCase);

			if (property == null)
				return defaultValue;

			try
			{
				object proposed = property.Value;

				if (property.Value is JValue val)
				{
					if (val.Value == null)
						return defaultValue;

					proposed = val.Value;
				};

				T r;

				if (typeof(T).IsSubclassOf(typeof(JToken)))
				{
					if (Types.TryConvert(proposed, out r))
						return r;
				}
				else
					return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(proposed));

				return default;
			}
			catch
			{
				throw new TomPITException(string.Format("{0} ({1}, {2}).", SR.ErrInvalidPropertyType, propertyName, typeof(T)));
			}
		}

		public static T WithExisting<T>(this JObject instance, string propertyName, JObject existing)
		{
			var property = instance.Property(propertyName, StringComparison.OrdinalIgnoreCase);

			if (property == null)
				return existing.Optional(propertyName, default(T));

			try
			{
				object proposed = property.Value;

				if (property.Value is JValue val)
				{
					if (val.Value == null)
						return default(T);

					proposed = val.Value;
				};

				if (Types.TryConvert(proposed, out T r))
					return r;

				return default(T);
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
				if (!(i is JObject jo))
					continue;

				var leftProperty = jo.Property(masterKey, StringComparison.OrdinalIgnoreCase);

				if (leftProperty == null)
					continue;

				var leftCriteria = leftProperty.Value;

				foreach (var j in detailsData)
				{
					if (!(j is JObject jor))
						continue;

					var rightProperty = jo.Property(detailsKey, StringComparison.OrdinalIgnoreCase);

					if (rightProperty == null)
						continue;

					var rightCriteria = rightProperty.Value;

					if (leftCriteria != rightCriteria)
						continue;

					foreach (var k in properties)
					{
						var prop = jor.Property(k, StringComparison.OrdinalIgnoreCase);

						if (prop == null)
							continue;

						var val = prop.Value;
						var p = jo.Property(k, StringComparison.OrdinalIgnoreCase);

						if (p != null)
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
				var prop = jo.Property(masterKey, StringComparison.OrdinalIgnoreCase);

				if (prop == null)
					continue;

				var masterValue = ((JValue)prop.Value).Value;

				foreach (var j in detailsData)
				{
					var jjo = j as JObject;
					var jp = jjo.Property(detailsKey);

					if (jp == null)
						continue;

					var detailValue = ((JValue)jp.Value).Value;

					if (Types.Compare(masterValue, detailValue))
					{
						jo.Add(detailsProperty, j);
						break;
					}
				}
			}
		}

		public static void JoinMany(this JArray masterData, string masterKey, JArray detailsData, string detailsKey, string detailsProperty)
		{
			if (masterData.Count == 0 || detailsData.Count == 0)
				return;

			foreach (var i in masterData)
			{
				var jo = i as JObject;
				var jop = jo.Property(masterKey, StringComparison.OrdinalIgnoreCase);

				if (jop == null)
					continue;

				var masterProp = jo.Property(masterKey, StringComparison.OrdinalIgnoreCase);

				if (masterProp == null)
					continue;

				var masterValue = ((JValue)masterProp.Value).Value;
				var a = new JArray();
				jo.Add(detailsProperty, a);

				foreach (var j in detailsData)
				{
					var jjo = j as JObject;
					var jjp = jjo.Property(detailsKey, StringComparison.OrdinalIgnoreCase);

					if (jjp == null)
						continue;

					var detailValue = ((JValue)jjp.Value).Value;

					if (Types.Compare(masterValue, detailValue))
						a.Add(j);
				}
			}
		}

		public static void JoinMany(this JObject masterData, string masterKey, JObject detailsData, string detailsKey, string detailsProperty)
		{
			JoinMany(ToResults(masterData), masterKey, ToResults(detailsData), detailsKey, detailsProperty);
		}

		public static void JoinMany(this JObject masterData, string masterKey, JArray detailsData, string detailsKey, string detailsProperty)
		{
			JoinMany(ToResults(masterData), masterKey, detailsData, detailsKey, detailsProperty);
		}

		public static void JoinMany(this JArray masterData, string masterKey, JObject detailsData, string detailsKey, string detailsProperty)
		{
			JoinMany(masterData, masterKey, ToResults(detailsData), detailsKey, detailsProperty);
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

		public static List<object> ToList(this JObject dataSource, string propertyName)
		{
			if (IsEmpty(dataSource))
				return new List<object>();

			return ToList(ToResults(dataSource), propertyName);
		}

		public static JArray ToArray(this JObject dataSource, string propertyName)
		{
			var r = new JArray();
			var results = ToResults(dataSource);
			var list = new List<object>();

			foreach (var i in results)
			{
				if (!(i is JObject jo))
					continue;

				var jop = jo.Property(propertyName, StringComparison.OrdinalIgnoreCase);

				if (jop == null)
					continue;

				var value = ((JValue)jop.Value).Value;

				if (!Types.TryConvert(value, out string sv) || string.IsNullOrWhiteSpace(sv))
					continue;

				if (!list.Contains(value))
				{
					list.Add(value);
					r.Add(value);
				}
			}

			return r;
		}

		public static List<object> ToList(this JObject dataSource)
		{
			var array = ToResults(dataSource);

			return ToList(array);
		}

		public static List<object> ToList(this JArray items)
		{
			var r = new List<object>();

			foreach (var i in items)
			{
				if (!(i is JObject jo))
					continue;

				if (jo.Count == 0)
					continue;

				if (!(jo.First is JProperty first))
					continue;

				var value = ((JValue)first.Value).Value;

				if (!Types.TryConvert(value, out string sv) || string.IsNullOrWhiteSpace(sv))
					continue;

				if (!r.Contains(value))
					r.Add(value);
			}

			return r;
		}

		public static List<object> ToList(this JArray items, string propertyName)
		{
			var r = new List<object>();

			foreach (var i in items)
			{
				if (!(i is JObject jo))
					continue;

				var jop = jo.Property(propertyName, StringComparison.OrdinalIgnoreCase);

				if (jop == null)
					continue;

				var value = ((JValue)jop.Value).Value;

				if (!Types.TryConvert(value, out string sv) || string.IsNullOrWhiteSpace(sv))
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
			var arr = new JArray();
			var data = (JArray)dataSource["data"];

			foreach (var i in data)
			{
				if (!(i is JObject jo))
					continue;

				var jop = jo.Property(propertyName, StringComparison.OrdinalIgnoreCase);

				if (jop == null)
					continue;

				var pv = ((JValue)jop.Value).Value;

				foreach (var j in values)
				{
					if (Types.Compare(pv, j))
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
			var criteria = "null";

			if (value != null)
			{
				if (value.GetType().IsNumericType() || value is bool)
					criteria = "{0}";
				else
					criteria = "'{0}'";
			}

			if (!(dataSource.SelectToken(string.Format("$.data[?(@.{0}=={1})]", propertyName, string.Format(criteria, value))) is JObject r))
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
			{
				if (!(i is JObject jo))
					continue;

				var prop = jo.Property(propertyName);

				if (prop == null)
					continue;

				result.Add(new JObject
				{
					{propertyName, prop.Value}
				});
			}

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

		public static void RemoveProperties(this JObject dataSource, params string[] properties)
		{
			RemoveProperties(dataSource.ToResults());
		}

		public static void RemoveProperties(this JArray items, params string[] properties)
		{
			foreach (var i in items)
			{
				var jo = i as JObject;

				foreach (var j in properties)
				{
					var p = jo.Property(j);

					if (p != null)
						jo.Remove(j);
				}
			}
		}
	}
}
