using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Development;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Ide.VersionControl
{
	internal class VersionControlService : TenantObject, IVersionControlService
	{
		public VersionControlService(ITenant tenant) : base(tenant)
		{

		}
		public List<IComponent> Changes()
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryChanges");

			return Tenant.Post<List<Component>>(u).ToList<IComponent>();
		}

		public List<IComponent> Changes(Guid microService)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryChanges");
			var e = new JObject
				{
					 {"microService", microService }
				};

			return Tenant.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public List<IComponent> Changes(Guid microService, Guid user)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryChanges");
			var e = new JObject
				{
					 {"microService", microService },
					 {"user", user }
				};

			return Tenant.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public void Commit(List<Guid> components, string comment)
		{
			var u = Tenant.CreateUrl("VersionControl", "Commit");
			var e = new JObject
				{
					 {"user", MiddlewareDescriptor.Current.UserToken },
					 {"comment", comment }
				};

			var a = new JArray();

			e.Add("components", a);

			foreach (var i in components)
				a.Add(i);

			Tenant.Post(u, e);
		}

		public ILockInfo SelectLockInfo(Guid component)
		{
			var u = Tenant.CreateUrl("VersionControl", "SelectLockInfo");
			var e = new JObject
				{
					 {"component", component },
					 {"user", MiddlewareDescriptor.Current.UserToken }
				};

			return Tenant.Post<LockInfo>(u, e);
		}

		public List<IComponent> QueryCommitComponents(Guid commit)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryCommitComponents");
			var e = new JObject
				{
					 {"commit", commit }
				};

			return Tenant.Post<List<Component>>(u, e).ToList<IComponent>();
		}

		public List<ICommit> QueryCommits(Guid microService)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryCommits");
			var e = new JObject
				{
					 {"microService", microService }
				};

			return Tenant.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public List<ICommit> QueryCommits(Guid microService, Guid user)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryCommits");
			var e = new JObject
				{
					 {"microService", microService },
					 {"user", user }
				};

			return Tenant.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public List<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryCommitsForComponent");
			var e = new JObject
				{
					 {"microService", microService },
					 {"component", component }
				};

			return Tenant.Post<List<Commit>>(u, e).ToList<ICommit>();
		}

		public void Rollback(Guid commit, Guid component)
		{
			var comps = QueryCommitDetails(commit);

			if (component != Guid.Empty)
				comps = comps.Where(f => f.Component == component).ToList();

			foreach (var i in comps)
			{
				Tenant.GetService<IComponentDevelopmentService>().RestoreComponent(i.Blob);

				var u = Tenant.CreateUrl("VersionControl", "Undo");
				var e = new JObject
					 {
						  {"component", i.Component }
					 };

				Tenant.Post(u, e);
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
				var component = Tenant.GetService<IComponentService>().SelectComponent(i);

				var history = QueryHistory(i);

				if (history.Count == 0)
					continue;

				var activeLock = history.FirstOrDefault(f => f.Commit == Guid.Empty);

				if (activeLock == null)
					continue;

				if (component.LockVerb == LockVerb.Edit || component.LockVerb == LockVerb.Delete)
					Tenant.GetService<IComponentDevelopmentService>().RestoreComponent(activeLock.Blob);

				Tenant.GetService<IStorageService>().Delete(activeLock.Blob);

				var u = Tenant.CreateUrl("VersionControl", "Undo");
				var e = new JObject
					 {
						  {"component", i }
					 };

				Tenant.Post(u, e);

				if (component.LockVerb == LockVerb.Add)
					Tenant.GetService<IComponentDevelopmentService>().Delete(i, true);
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
					var user = Tenant.GetService<IUserService>().Select(lockInfo.Owner.ToString());

					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrVcLocked, user.DisplayName()));
			}

			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var token = Guid.NewGuid();
			var ms = Tenant.GetService<IMicroServiceService>().Select(c.MicroService);
			var image = Tenant.GetService<IComponentDevelopmentService>().CreateComponentImage(component);

			var blob = Tenant.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", c.Name),
				MicroService = c.MicroService,
				PrimaryKey = token.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.ComponentHistory
			}, Tenant.GetService<ISerializationService>().Serialize(image), StoragePolicy.Singleton);

			var u = Tenant.CreateUrl("VersionControl", "Lock");
			var e = new JObject
				{
					 { "token", token },
					 { "component", component },
					 { "user", MiddlewareDescriptor.Current.UserToken },
					 { "blob", blob },
					 { "verb", verb.ToString() }
				};

			Tenant.Post(u, e);
		}

		public List<IComponentHistory> QueryHistory(Guid component)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryHistory");
			var e = new JObject
				{
					 {"component", component }
				};

			return Tenant.Post<List<ComponentHistory>>(u, e).ToList<IComponentHistory>();
		}

		public List<IComponentHistory> QueryCommitDetails(Guid commit)
		{
			var u = Tenant.CreateUrl("VersionControl", "QueryCommitDetails");
			var e = new JObject
				{
					 {"commit", commit }
				};

			return Tenant.Post<List<ComponentHistory>>(u, e).ToList<IComponentHistory>();
		}
	}
}