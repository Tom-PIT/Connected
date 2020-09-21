using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using TomPIT.Annotations.Search;
using TomPIT.Data;

namespace TomPIT.Search
{
	public class SearchEntity : DataEntity, ISearchEntity
	{
		private List<ISearchField> _properties = null;

		[JsonConverter(typeof(SearchPropertiesConverter))]
		public List<ISearchField> Properties
		{
			get
			{
				if (_properties == null)
					_properties = new List<ISearchField>();

				return _properties;
			}
		}

		[SearchStore(true)]
		[SearchMode(SearchMode.NotAnalyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Type { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.NotAnalyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Key { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public int Lcid { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		[SearchBoost(4)]
		public string Title { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		[SearchBoost(2)]
		public string Text { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		[SearchBoost(3)]
		public string Tags { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.Analyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public string Author { get; set; }

		[SearchStore(true)]
		[SearchMode(SearchMode.NotAnalyzed)]
		[SearchTermVector(SearchTermVector.No)]
		public DateTime Date { get; set; }

		public T GetProperty<T>(string name)
		{
			var property = GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (property != null)
			{
				if (Types.TryConvert(property.GetValue(this), out T result))
					return result;

				return default;
			}

			foreach (var prop in Properties)
			{
				if (string.Compare(prop.Name, name, false) == 0)
				{
					if (Types.TryConvert(property.GetValue(this), out T result))
						return result;

					return default;
				}
			}

			return default;
		}
	}
}