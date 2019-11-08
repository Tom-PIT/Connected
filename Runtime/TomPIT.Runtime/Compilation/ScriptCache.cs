using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Compilation
{
	internal class ScriptCache : ConfigurationRepository<IScriptConfiguration>
	{
		public ScriptCache(ITenant tenant) : base(tenant, "iscript")
		{
		}

		protected override string[] Categories => ComponentCategories.ScriptCategories;

		public List<IScriptConfiguration> Items => All();
	}
}
