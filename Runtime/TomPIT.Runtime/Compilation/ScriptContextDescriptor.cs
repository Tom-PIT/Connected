using System;
using System.Collections.Generic;

namespace TomPIT.Compilation
{
	internal class ScriptContextDescriptor
	{
		private HashSet<string> _loadReferences = null;

		public HashSet<string> LoadReferences
		{
			get
			{
				if (_loadReferences == null)
					_loadReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				
				return _loadReferences;
			}
		}
	}
}