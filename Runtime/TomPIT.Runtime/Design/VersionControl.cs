using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Development;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Design
{
	internal class VersionControl : TenantObject, IVersionControl
	{
		private const string Controller = "VersionControl";
		public VersionControl(ITenant tenant) : base(tenant)
		{

		}
		public List<IComponent> Changes()
		{
			return Tenant.Post<List<Component>>(CreateUrl("QueryChanges")).ToList<IComponent>();
		}

		public void DeleteHistory(Guid component)
		{
			Tenant.Post(CreateUrl("DeleteHistory"), new
			{
				component
			});
		}

		public ICommit SelectCommit(Guid token)
		{
			return Tenant.Post<Commit>(CreateUrl("SelectCommit"), new
			{
				token
			});
		}

		public IComponentHistory SelectNonCommited(Guid component)
		{
			return Tenant.Post<ComponentHistory>(CreateUrl("SelectNonCommited"), new
			{
				Component = component
			});
		}

		public List<IComponent> Changes(Guid microService)
		{
			return Tenant.Post<List<Component>>(CreateUrl("QueryChanges"), new
			{
				MicroService = microService
			}).ToList<IComponent>();
		}

		public List<IComponent> Changes(Guid microService, Guid user)
		{
			return Tenant.Post<List<Component>>(CreateUrl("QueryChanges"), new
			{
				MicroService = microService,
				User = user
			}).ToList<IComponent>();
		}

		public void Commit(List<Guid> components, string comment)
		{
			Tenant.Post(CreateUrl("Commit"), new
			{
				User = MiddlewareDescriptor.Current.UserToken,
				Comment = comment,
				Components = components
			});
		}

		public ILockInfo SelectLockInfo(Guid component)
		{
			return Tenant.Post<LockInfo>(CreateUrl("SelectLockInfo"), new
			{
				Component = component,
				User = MiddlewareDescriptor.Current.UserToken
			});
		}

		public List<IComponent> QueryCommitComponents(Guid commit)
		{
			return Tenant.Post<List<Component>>(CreateUrl("QueryCommitComponents"), new
			{
				Commit = commit
			}).ToList<IComponent>();
		}

		public List<ICommit> QueryCommits()
		{
			return Tenant.Post<List<Commit>>(CreateUrl("QueryCommits")).ToList<ICommit>();
		}

		public List<ICommit> QueryCommits(Guid microService)
		{
			return Tenant.Post<List<Commit>>(CreateUrl("QueryCommits"), new
			{
				MicroService = microService
			}).ToList<ICommit>();
		}

		public List<ICommit> QueryCommits(Guid microService, Guid user)
		{
			return Tenant.Post<List<Commit>>(CreateUrl("QueryCommits"), new
			{
				MicroService = microService,
				User = user
			}).ToList<ICommit>();
		}

		public List<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
		{
			return Tenant.Post<List<Commit>>(CreateUrl("QueryCommitsForComponent"), new
			{
				MicroService = microService,
				Component = component
			}).ToList<ICommit>();
		}

		public void Rollback(Guid commit, Guid component)
		{
			var comps = QueryCommitDetails(commit);

			if (component != Guid.Empty)
				comps = comps.Where(f => f.Component == component).ToList();

			foreach (var i in comps)
			{
				Tenant.GetService<IDesignService>().Components.RestoreComponent(i.Blob);

				Tenant.Post(CreateUrl("Undo"), new
				{
					Component = component
				});
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
					Tenant.GetService<IDesignService>().Components.RestoreComponent(activeLock.Blob);

				Tenant.GetService<IStorageService>().Delete(activeLock.Blob);

				Tenant.Post(CreateUrl("Undo"), new
				{
					Component = i
				});

				if (component.LockVerb == LockVerb.Add)
					Tenant.GetService<IDesignService>().Components.Delete(i, true);
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
			var image = Tenant.GetService<IDesignService>().Components.CreateComponentImage(component);

			var blob = Tenant.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", c.Name),
				MicroService = c.MicroService,
				PrimaryKey = token.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.ComponentHistory
			}, Tenant.GetService<ISerializationService>().Serialize(image), StoragePolicy.Singleton);

			Tenant.Post(CreateUrl("Lock"), new
			{
				Token = token,
				Component = component,
				User = MiddlewareDescriptor.Current.UserToken,
				Blob = blob,
				Verb = verb.ToString()
			});
		}

		public List<IComponentHistory> QueryHistory(Guid component)
		{
			/*
			 * this call will throw exception if component doesn't exist. when called
			 * from version control this might be a common case so we'll eat up
			 * an exception
			 */
			try
			{
				return Tenant.Post<List<ComponentHistory>>(CreateUrl("QueryHistory"), new
				{
					component
				}).ToList<IComponentHistory>();
			}
			catch
			{
				return new List<IComponentHistory>();
			}
		}

		public List<IComponentHistory> QueryMicroServiceHistory(Guid microService)
		{
			return Tenant.Post<List<ComponentHistory>>(CreateUrl("QueryMicroServiceHistory"), new
			{
				microService
			}).ToList<IComponentHistory>();
		}

		public List<IComponentHistory> QueryCommitDetails(Guid commit)
		{
			return Tenant.Post<List<ComponentHistory>>(CreateUrl("QueryCommitDetails"), new
			{
				Commit = commit
			}).ToList<IComponentHistory>();
		}

		public IComponentHistory SelectCommitDetail(Guid commit, Guid component)
		{
			return Tenant.Post<ComponentHistory>(CreateUrl("SelectCommitDetail"), new
			{
				Commit = commit,
				Component = component
			});
		}

		public IChangeDescriptor GetChanges(ChangeQueryMode mode)
		{
			return GetChanges(mode, Guid.Empty);
		}
		public IChangeDescriptor GetChanges(ChangeQueryMode mode, Guid user)
		{
			return GetChanges(mode, user, Guid.Empty);
		}
		public IChangeDescriptor GetChanges(ChangeQueryMode mode, Guid user, Guid commit)
		{
			var result = new ChangeDescriptor();
			List<IComponent> changes = commit == Guid.Empty ? Changes(Guid.Empty, user) : ResolveCommitChanges(commit);

			var groups = changes.GroupBy(f => f.MicroService);

			foreach (var group in groups)
				result.MicroServices.Add(CreateMicroserviceChanges(group, mode, commit));

			return result;
		}

		private List<IComponent> ResolveCommitChanges(Guid commit)
		{
			var components = QueryCommitDetails(commit);
			var descriptor = SelectCommit(commit);
			var result = new List<IComponent>();

			foreach(var component in components)
			{
				var image = Tenant.GetService<IDesignService>().Components.SelectComponentImage(component.Blob);

				var c = new Component
				{
					LockVerb = component.Verb,
					LockUser = component.User,
					MicroService = descriptor.Service,
					Name = component.Name,
					Token = component.Component
				};

				if(image != null)
				{
					c.Folder = image.Folder;
					c.Category = image.Category;
					c.Type = image.Type;
				}

				result.Add(c);
			}

			return result;
		}

		private IChangeMicroService CreateMicroserviceChanges(IGrouping<Guid, IComponent> group, ChangeQueryMode mode, Guid commit)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(group.Key);
			var result = new ChangeMicroService
			{
				Name = ms.Name,
				Id = ms.Token
			};

			if (commit == Guid.Empty)
			{
				var folders = Tenant.GetService<IComponentService>().QueryFolders(ms.Token);

				foreach (var folder in folders)
				{
					result.Folders.Add(new ChangeFolder
					{
						Name = folder.Name,
						Parent = folder.Parent,
						Id = folder.Token
					});
				}
			}

			foreach (var component in group)
				result.Components.Add(new ComponentParser(Tenant, component, mode, commit).Parse());

			return result;
		}

		public IDiffDescriptor GetDiff(Guid component, Guid id, Guid commit)
		{
			if (commit == Guid.Empty)
				return GetDiff(component, id);

			var history = QueryHistory(component).OrderByDescending(f => f.Created);
			var target = history.FirstOrDefault(f => f.Commit == commit);

			if (target == null)
				return null;

			var next = history.OrderBy(f => f.Created).FirstOrDefault(f => f.Commit != commit && f.Created > target.Created);
			var targetImage = Tenant.GetService<IDesignService>().Components.SelectComponentImage(target.Blob);

			if (targetImage == null)
				return null;

			IComponentImageBlob targetBlob = null;
			IComponentImageBlob nextBlob = null;

			if (id == Guid.Empty)
				targetBlob = targetImage.Configuration;
			else
				targetBlob = targetImage.Dependencies.FirstOrDefault(f => f.Token == id);

			if (next != null)
			{
				var nextImage = Tenant.GetService<IDesignService>().Components.SelectComponentImage(next.Blob);

				if (nextImage != null)
				{
					if (id == Guid.Empty)
						nextBlob = nextImage.Configuration;
					else
						nextBlob = nextImage.Dependencies.FirstOrDefault(f => f.Token == id);
				}
			}
			else
			{
				/*
				 * current
				 */
				nextBlob = new ComponentImageBlob
				{
					Content = Tenant.GetService<IStorageService>().Download(id)?.Content
				};
			}

			var syntax = "json";

			if (id != Guid.Empty)
			{
				var type = TypeExtensions.GetType(targetImage.Type);

				if (type != null && Tenant.GetService<ISerializationService>().Deserialize(targetImage.Configuration.Content, type) is IConfiguration config)
				{
					var element = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(config).FirstOrDefault(f => f.TextBlob == id);

					if (element != null)
					{
						var stx = element.GetType().FindAttribute<SyntaxAttribute>();
						syntax = stx == null ? SyntaxAttribute.CSharp : stx.Syntax;
					}
				}
			}

			return new DiffDescriptor
			{
				Modified = nextBlob == null || nextBlob.Content == null || nextBlob.Content.Length	 == 0 ? string.Empty : Encoding.UTF8.GetString(nextBlob.Content),
				Original = targetBlob == null || targetBlob.Content == null || targetBlob.Content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(targetBlob.Content),
				Syntax = syntax
			};
		}

		public IDiffDescriptor GetDiff(Guid component, Guid id)
		{
			if (id == Guid.Empty)
				return GetComponentDiff(component);
			else
				return GetConfigurationDiff(component, id);
		}

		private IDiffDescriptor GetComponentDiff(Guid component)
		{
			string modified = string.Empty;
			string original = string.Empty;

			var modifiedRaw = Tenant.GetService<IStorageService>().Download(component);

			if (modifiedRaw != null && modifiedRaw.Content != null)
				modified = Encoding.UTF8.GetString(modifiedRaw.Content);

			var unCommited = SelectNonCommited(component);

			if (unCommited != null)
			{
				var originalImage = Tenant.GetService<IDesignService>().Components.SelectComponentImage(unCommited.Blob);

				if (originalImage != null)
					original = Encoding.UTF8.GetString(originalImage.Configuration.Content);
			}

			return new DiffDescriptor
			{
				Original = original,
				Modified = modified,
				Syntax = "json"
			};
		}

		private IDiffDescriptor GetConfigurationDiff(Guid component, Guid id)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

			if (config == null)
				return null;

			var target = Tenant.GetService<IDiscoveryService>().Children<IText>(config).FirstOrDefault(f => f.TextBlob == id);

			if (target == null)
				return null;

			var syntax = target.GetType().FindAttribute<SyntaxAttribute>();

			return new DiffDescriptor
			{
				Modified = Tenant.GetService<IComponentService>().SelectText(config.MicroService(), target),
				Syntax = syntax == null ? SyntaxAttribute.CSharp : syntax.Syntax,
				Original = ResolveOriginal(config, id)
			};
		}
		private string ResolveOriginal(IConfiguration config, Guid id)
		{
			var unCommited = SelectNonCommited(config.Component);

			if (unCommited == null)
				return null;

			var image = Tenant.GetService<IDesignService>().Components.SelectComponentImage(unCommited.Blob);

			if (image == null)
				return null;

			var dep = image.Dependencies.FirstOrDefault(f => f.Token == id);

			if (dep == null)
				return null;

			return Encoding.UTF8.GetString(dep.Content);
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl(Controller, action);
		}
	}
}