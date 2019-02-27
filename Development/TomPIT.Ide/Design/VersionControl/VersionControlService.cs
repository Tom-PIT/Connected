using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Design.Serialization;
using TomPIT.Development;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Ide.Design.VersionControl
{
	internal class VersionControlService : IVersionControlService
	{
		public VersionControlService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public List<IComponent> Changes(Guid microService)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryChanges");
			var e = new JObject
			{
				{"microService", microService }
			};

			return Connection.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public List<IComponent> Changes(Guid microService, Guid user)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryChanges");
			var e = new JObject
			{
				{"microService", microService },
				{"user", user }
			};

			return Connection.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public void Commit(List<Guid> components, string comment)
		{
			var u = Connection.CreateUrl("VersionControl", "Commit");
			var e = new JObject
			{
				{"user", Shell.HttpContext.CurrentUserToken() },
				{"comment", comment }
			};

			var a = new JArray();

			e.Add("components", a);

			foreach (var i in components)
				a.Add(i);

			Connection.Post(u, e);
		}

		public ILockInfo SelectLockInfo(Guid component)
		{
			var u = Connection.CreateUrl("VersionControl", "SelectLockInfo");
			var e = new JObject
			{
				{"component", component },
				{"user", Shell.HttpContext.CurrentUserToken() }
			};

			return Connection.Post<LockInfo>(u, e);
		}

		public List<IComponent> QueryCommitComponents(Guid commit)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryCommitComponents");
			var e = new JObject
			{
				{"commit", commit }
			};

			return Connection.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public List<ICommit> QueryCommits(Guid microService)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryCommits");
			var e = new JObject
			{
				{"microService", microService }
			};

			return Connection.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public List<ICommit> QueryCommits(Guid microService, Guid user)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryCommits");
			var e = new JObject
			{
				{"microService", microService },
				{"user", user }
			};

			return Connection.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public List<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryCommitsForComponent");
			var e = new JObject
			{
				{"microService", microService },
				{"component", component }
			};

			return Connection.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public void Rollback(Guid commit, Guid component)
		{
			var comps = QueryCommitDetails(commit);

			if (component != Guid.Empty)
				comps = comps.Where(f => f.Component == component).ToList();

			foreach (var i in comps)
			{
				Connection.GetService<IComponentDevelopmentService>().RestoreComponent(i.Blob);

				var u = Connection.CreateUrl("VersionControl", "Undo");
				var e = new JObject
				{
					{"component", i.Component }
				};

				Connection.Post(u, e);
			}
		}

		public void Rollback(Guid commit)
		{
			Rollback(commit, Guid.Empty);
		}

		public void Undo(List<Guid> components)
		{
			foreach (var i in components)
			{
				var component = Connection.GetService<IComponentService>().SelectComponent(i);

				var history = QueryHistory(i);

				if (history.Count == 0)
					continue;

				var activeLock = history.FirstOrDefault(f => f.Commit == Guid.Empty);

				if (activeLock == null)
					continue;

				if (component.LockVerb == LockVerb.Edit || component.LockVerb == LockVerb.Delete)
					Connection.GetService<IComponentDevelopmentService>().RestoreComponent(activeLock.Blob);

				Connection.GetService<IStorageService>().Delete(activeLock.Blob);

				var u = Connection.CreateUrl("VersionControl", "Undo");
				var e = new JObject
				{
					{"component", i }
				};

				Connection.Post(u, e);

				if (component.LockVerb == LockVerb.Add)
					Connection.GetService<IComponentDevelopmentService>().Delete(i, true);
			}
		}

		public void Lock(Guid component, LockVerb verb)
		{
			var lockInfo = SelectLockInfo(component);

			switch (lockInfo.Result)
			{
				case LockInfoResult.NoAction:
					return;
				case LockInfoResult.Locked:
					var user = Connection.GetService<IUserService>().Select(lockInfo.Owner.ToString());

					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrVcLocked, user.DisplayName()));
			}

			var c = Connection.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var token = Guid.NewGuid();
			var ms = Connection.GetService<IMicroServiceService>().Select(c.MicroService);
			var image = Connection.GetService<IComponentDevelopmentService>().CreateComponentImage(component);

			var blob = Connection.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", c.Name),
				MicroService = c.MicroService,
				PrimaryKey = token.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.ComponentHistory
			}, Connection.GetService<ISerializationService>().Serialize(image), StoragePolicy.Singleton);

			var u = Connection.CreateUrl("VersionControl", "Lock");
			var e = new JObject
			{
				{ "token", token },
				{ "component", component },
				{ "user", Shell.HttpContext.CurrentUserToken() },
				{ "blob", blob },
				{ "verb", verb.ToString() }
			};

			Connection.Post(u, e);
		}

		public List<IComponentHistory> QueryHistory(Guid component)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryHistory");
			var e = new JObject
			{
				{"component", component }
			};

			return Connection.Post<List<ComponentHistory>>(u, e).ToList<IComponentHistory>();
		}

		public List<IComponentHistory> QueryCommitDetails(Guid commit)
		{
			var u = Connection.CreateUrl("VersionControl", "QueryCommitDetails");
			var e = new JObject
			{
				{"commit", commit }
			};

			return Connection.Post<List<ComponentHistory>>(u, e).ToList<IComponentHistory>();
		}
	}
}