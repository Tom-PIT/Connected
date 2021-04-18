using System.Collections.Generic;

namespace TomPIT.BigData.Persistence
{
	internal class IndexArrayParameter : IndexParameter
	{
		private List<object> _values = null;
		public List<object> Values
		{
			get
			{
				if (_values == null)
					_values = new List<object>();

				return _values;
			}
		}
	}
}
