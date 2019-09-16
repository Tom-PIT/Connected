using System.Collections.Generic;

namespace TomPIT.Ide.Collections
{
	public class SelectItems : List<Option>
	{
		public void Add(string text, string value)
		{
			Add(new Option(text, value));
		}
	}
}
