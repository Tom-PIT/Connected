using System;
using System.Collections.Generic;

namespace TomPIT.Search
{
	public interface ISearchEntity
	{
		public string Key { get; }
		public string Type { get; }
		public int Lcid { get; }
		public string Title { get; }
		public string Text { get; }
		public string Tags { get; }
		public string Author { get; }
		public DateTime Date { get; }

		List<ISearchField> Properties { get; }

		T GetProperty<T>(string name);
	}
}
