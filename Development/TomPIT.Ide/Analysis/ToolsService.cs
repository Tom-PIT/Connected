using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Analysis;
using TomPIT.Connectivity;
using TomPIT.Ide.Analysis.Tools;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis
{
	internal class ToolsService : TenantObject, IToolsService
	{
		private Lazy<List<IToolMiddleware>> _items = new Lazy<List<IToolMiddleware>>();
		public ToolsService(ITenant tenant) : base(tenant)
		{
			Register(new ConfigurationTypeLoader());
		}

		public List<ITool> Query()
		{
			var u = Tenant.CreateUrl("Tools", "Query");

			return Tenant.Get<List<Tool>>(u).ToList<ITool>();
		}

		public void Run(string name)
		{
			UpdateStatus(name, ToolStatus.Pending);
		}

		public void Activate(string name)
		{
			UpdateStatus(name, ToolStatus.Running);
		}

		public void Complete(string name)
		{
			UpdateStatus(name, ToolStatus.Idle);
		}

		private void UpdateStatus(string name, ToolStatus status)
		{
			var u = Tenant.CreateUrl("Tools", "Update");
			var e = new JObject
			{
				{"name", name },
				{"status", status.ToString() }
			};

			Tenant.Post(u, e);
		}
		public ITool Select(string name)
		{
			var u = Tenant.CreateUrl("Tools", "Select");
			var e = new JObject
			{
				{"name", name }
			};

			return Tenant.Post<Tool>(u, e);
		}

		public void Register(IToolMiddleware middleware)
		{
			Tools.Add(middleware);
		}

		public IToolMiddleware GetTool(string name)
		{
			return Tools.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
		}

		private List<IToolMiddleware> Tools => _items.Value;
	}
}
