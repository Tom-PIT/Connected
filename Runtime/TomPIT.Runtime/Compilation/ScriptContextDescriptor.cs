using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class ScriptContextDescriptor
	{
		private List<string> _references = null;

		public List<string> References
		{
			get
			{
				if (_references == null)
					_references = new List<string>();

				return _references;
			}
		}
	}
}