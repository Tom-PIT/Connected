using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class VersionControlController : SysController
	{
		[HttpPost]
		public List<IComponent> QueryChanges()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var u = body.Optional("user", Guid.Empty);

			if (u != Guid.Empty)
				return DataModel.Components.QueryLocks(ms, u);
			else
				return DataModel.Components.QueryLocks(ms);
		}

		[HttpPost]
		public List<ICommit> QueryCommits()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var u = body.Optional("user", Guid.Empty);

			if (u != Guid.Empty)
				return DataModel.VersionControl.QueryCommits(ms, u);
			else
				return DataModel.VersionControl.QueryCommits(ms);
		}

		[HttpPost]
		public List<ICommit> QueryCommitsForComponent()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var c = body.Optional("component", Guid.Empty);

			return DataModel.VersionControl.QueryCommitsForComponent(ms, c);
		}

		[HttpPost]
		public List<IComponentHistory> QueryHistory()
		{
			var body = FromBody();
			var c = body.Required<Guid>("component");

			return DataModel.VersionControl.QueryHistory(c);
		}

		[HttpPost]
		public List<IComponentHistory> QueryCommitDetails()
		{
			var body = FromBody();
			var c = body.Required<Guid>("commit");

			return DataModel.VersionControl.QueryCommitDetails(c);
		}

		[HttpPost]
		public List<IComponent> QueryCommitComponents()
		{
			var body = FromBody();
			var commit = body.Required<Guid>("commit");

			return DataModel.VersionControl.QueryCommitComponents(commit);
		}

		[HttpPost]
		public void Commit()
		{
			var body = FromBody();
			var comment = body.Required<string>("comment");
			var u = body.Required<Guid>("user");
			var a = body.Required<JArray>("components");
			var components = new List<Guid>();

			foreach (JValue i in a)
				components.Add(Types.Convert<Guid>(i.Value));

			DataModel.Components.Commit(components, u, comment);
		}

		[HttpPost]
		public ILockInfo SelectLockInfo()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var user = body.Required<Guid>("user");

			return DataModel.VersionControl.SelectLockInfo(component, user);
		}

		[HttpPost]
		public void Lock()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var user = body.Required<Guid>("user");
			var blob = body.Required<Guid>("blob");

			DataModel.VersionControl.Lock(component, user, blob);
		}

		[HttpPost]
		public void Undo()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");

			DataModel.VersionControl.Undo(component);
		}
	}
}
