using System;
using System.Collections.Generic;

namespace TomPIT.Search.Catalogs
{
	public class SearchResult
	{
		private List<SearchResultField> _fields = null;
		public float Score { get; set; }
		public string Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public int Lcid { get; set; }
		public string Tags { get; set; }
		public Guid User { get; set; }
		public DateTime Date { get; set; }


		public List<SearchResultField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new List<SearchResultField>();

				return _fields;
			}
		}

		public static implicit operator SearchResultDescriptor(SearchResult d)
		{
			if (d == null)
				return null;

			var r = new SearchResultDescriptor
			{
				Title = d.Title,
				Lcid = d.Lcid,
				Tags = d.Tags,
				User = d.User,
				Date = d.Date,
				Content = d.Content,
				Id = d.Id
			};

			return r;
		}
	}
}