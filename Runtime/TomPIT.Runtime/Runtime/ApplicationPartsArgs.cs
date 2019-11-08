using System;
using System.Collections.Generic;
using System.Reflection;

namespace TomPIT.Runtime
{
	public class ApplicationPartsArgs : EventArgs
	{
		private List<string> _parts = null;
		private List<Assembly> _assemblies = null;

		public List<string> Parts
		{
			get
			{
				if (_parts == null)
					_parts = new List<string>();

				return _parts;

			}
		}

		public List<Assembly> Assemblies
		{
			get
			{
				if (_assemblies == null)
					_assemblies = new List<Assembly>();

				return _assemblies;
			}
		}
	}
}
