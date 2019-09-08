using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Compilers
{
	internal class ScriptCache : ConfigurationAwareService<IScript>
	{
		public ScriptCache(ISysConnection connection) : base(connection, "iscript")
		{
		}

		protected override string[] Categories => ComponentCategories.ScriptCategories;

		public List<IScript> Items => All();
	}
}
