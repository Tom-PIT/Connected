using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.BigData.Data
{
	[JsonConverter(typeof(PartitionSchemaFieldConverter))]
	internal abstract class PartitionSchemaField : IComparable
	{
		private List<Attribute> _attributes = null;
		public string Name { get; set; }
		public bool Key { get; set; }
		public bool Index { get; set; }

		public Type Type { get; set; }

		[JsonIgnore]
		public List<Attribute> Attributes
		{
			get
			{
				if (_attributes == null)
					_attributes = new List<Attribute>();

				return _attributes;
			}
		}

		public virtual int CompareTo(object obj)
		{
			var sf = obj as PartitionSchemaField;

			if (sf == null)
				return -1;

			var result = string.Compare(Name, sf.Name, true);

			if (result != 0)
				return result;

			if (Key != sf.Key)
				return -1;

			if (Index != sf.Index)
				return -1;

			if (Type != sf.Type)
				return -1;

			return 0;
		}
	}
}
