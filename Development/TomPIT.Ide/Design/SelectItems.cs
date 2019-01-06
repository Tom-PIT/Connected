using System.Collections.Generic;

namespace TomPIT.ComponentModel.Data
{
	public class SelectItems : List<Option>
	{
		public void Add(string text, string value)
		{
			Add(new Option(text, value));
		}
	}
}
