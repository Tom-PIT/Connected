using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Data
{
	public class DataList<T> : List<T> where T : DataEntity
	{

		public static implicit operator DataList<T>(JObject items)
		{
			return items.ToDataList<T>();
		}

		public static implicit operator DataList<T>(JArray items)
		{
			return items.ToDataList<T>();
		}
	}
}
