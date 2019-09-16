using System;
using System.Collections.Generic;

namespace TomPIT.Runtime
{
	public class ApplicationPartsArgs : EventArgs
	{
		private List<string> _parts = null;

		public List<string> Parts
		{
			get
			{
				if (_parts == null)
					_parts = new List<string>();

				return _parts;

			}
		}
	}
}
