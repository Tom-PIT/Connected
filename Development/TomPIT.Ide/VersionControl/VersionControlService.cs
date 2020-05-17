using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Development;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
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

		public List<IVersionControlDescriptor> GetChanges()
		{
			var result = new List<IVersionControlDescriptor>();
			var changes = Changes();

			var groups = changes.GroupBy(f => f.MicroService);

			foreach (var group in groups)
				result.Add(CreateMicroserviceChanges(group));

			return result;
		}

		private IVersionControlDescriptor CreateMicroserviceChanges(IGrouping<Guid, IComponent> group)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(group.Key);

			var result = new VersionControlDescriptor
			{
				Id = group.Key,
				Microservice = group.Key,
				Name = ms.Name
			};

			var folders = Tenant.GetService<IComponentService>().QueryFolders(ms.Token);

			foreach (var component in group)
			{
				var folder = CreateFolderTree(result, component, folders);

				if (folder == null)
					folder = result;

				folder.Items.Add(ResolveComponent(component));
			}

			return result;
		}

		private IVersionControlDescriptor CreateFolderTree(VersionControlDescriptor descriptor, IComponent component, List<IFolder> folders)
		{
			if (component.Folder == Guid.Empty)
				return null;

			var paths = CreateFolderPath(folders, component.Folder);
			var current = descriptor;

			while (paths.Count > 0)
			{
				var folder = paths.Pop();

				if (descriptor.Items.FirstOrDefault(f => f.Id == folder.Token) == null)
				{
					var folderDescriptor = new VersionControlDescriptor
					{
						Id = folder.Token,
						Folder = folder.Parent,
						Name = folder.Name,
						Microservice = folder.MicroService
					};

					current.Items.Add(folderDescriptor);
					current = folderDescriptor;
				}
			}

			return current;
		}

		private Stack<IFolder> CreateFolderPath(List<IFolder> folders, Guid target)
		{
			var result = new Stack<IFolder>();

			while (target != Guid.Empty)
			{
				var folder = folders.FirstOrDefault(f => f.Token == target);

				if (folder == null)
					return null;

				result.Push(folder);

				target = folder.Parent;
			}

			return result;
		}

		private IVersionControlDescriptor ResolveComponent(IComponent component)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			var descriptor = new VersionControlDescriptor
			{
				Folder = component.Folder,
				Id = component.Token,
				Name = component.Name,
				Syntax = ResolveSyntax(config),
				Microservice = component.MicroService,
				Component = component.Token
			};

			if (config is IText text)
				descriptor.Blob = text.TextBlob;

			var texts = config.Children<IText>();

			foreach (var txt in texts)
			{
				if (txt == config)
					continue;

				CreateTextChain(descriptor, txt);
			}

			return descriptor;
		}

		private void CreateTextChain(IVersionControlDescriptor descriptor, IText txt)
		{
			var chains = new Stack<IElement>();
			var current = txt.Parent;

			while (current != null)
			{
				if (current is IConfiguration)
					break;

				chains.Push(current);

				current = current.Parent;
			}

			var currentDescriptor = descriptor;

			while (chains.Count > 0)
			{
				var chain = chains.Pop();

				var chainDescriptor = currentDescriptor.Items.FirstOrDefault(f => f.Id == chain.Id);

				if (chainDescriptor == null)
				{
					chainDescriptor = new VersionControlDescriptor
					{
						Id = chain.Id,
						Name = ResolveName(chain),
						Component = descriptor.Component,
						Microservice = descriptor.Microservice
					};

					currentDescriptor.Items.Add(chainDescriptor);
				}

				currentDescriptor = chainDescriptor;
			}

			currentDescriptor.Items.Add(new VersionControlDescriptor
			{
				Blob = txt.TextBlob,
				Id = txt.Id,
				Name = txt.ToString(),
				Component = descriptor.Component,
				Microservice = descriptor.Microservice,
				Syntax = ResolveSyntax(txt)
			});
		}

		private string ResolveName(IElement element)
		{
			if (element.Parent == null || element.Parent.GetType().IsCollection())
				return element.ToString();

			var props = element.Parent.GetType().GetProperties();

			foreach (var property in props)
			{
				if (property.GetType().IsCollection())
					continue;

				var value = property.GetValue(element.Parent);

				if (value == element)
					return property.Name;
			}

			return element.ToString();
		}
		private string ResolveSyntax(object text)
		{
			var syntax = text.GetType().FindAttribute<SyntaxAttribute>();

			return syntax == null ? SyntaxAttribute.CSharp : syntax.Syntax;
		}

		public IVersionControlDiffDescriptor GetDiff(Guid component, Guid blob)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

			if (config == null)
				return null;

			var target = config.Children<IText>().FirstOrDefault(f => f.TextBlob == blob);

			if (target == null)
				return null;

			var syntax = target.GetType().FindAttribute<SyntaxAttribute>();

			return new VersionControlDiffDescriptor
			{
				Modified = Tenant.GetService<IComponentService>().SelectText(config.MicroService(), target),
				Syntax = syntax == null ? SyntaxAttribute.CSharp : syntax.Syntax,
				Original = string.Empty/*TODO:connect to the server*/
			};
		}
	}
}