using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Api.ComponentModel;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
    internal class Components : SynchronizedRepository<IComponent, Guid>
    {
        public Components(IMemoryCache container) : base(container, "component")
        {
        }

        protected override void OnInitializing()
        {
            var ds = Shell.GetService<IDatabaseService>().Proxy.Development.Components.Query();

            foreach (var i in ds)
                Set(i.Token, i, TimeSpan.Zero);
        }

        protected override void OnInvalidate(Guid id)
        {
            var r = Shell.GetService<IDatabaseService>().Proxy.Development.Components.Select(id);

            if (r == null)
            {
                Remove(id);
                return;
            }

            Set(id, r, TimeSpan.Zero);
        }

        public void Commit(List<Guid> components, Guid user, string comment)
        {
            var changes = new Dictionary<Guid, List<IComponent>>();

            foreach (var i in components)
            {
                var component = Select(i);

                if (component == null)
                    throw new SysException(SR.ErrComponentNotFound);

                if (component.LockStatus != LockStatus.Lock)
                    throw new SysException(SR.ErrComponentNotLocked);

                if (component.LockUser != user)
                    throw new SysException(SR.ErrComponentLockMismatch);

                if (changes.ContainsKey(component.MicroService))
                    changes[component.MicroService].Add(component);
                else
                    changes.Add(component.MicroService, new List<IComponent> { component });
            }

            foreach (var i in changes)
            {
                DataModel.VersionControl.InsertCommit(i.Key, user, comment, i.Value);

                foreach (var component in i.Value)
                {
                    if (component.LockVerb == LockVerb.Delete)
                        Delete(component.Token, user, true);
                    else
                    {
                        Refresh(component.Token);
                        CachingNotifications.ComponentChanged(component.MicroService, component.Folder, component.Token, component.Category);
                    }
                }

                var microService = DataModel.MicroServices.Select(i.Key);

                if (microService.Package == Guid.Empty)
                    continue;

                DataModel.MicroServices.Update(microService.Token, microService.Name, microService.Status, microService.Template, microService.ResourceGroup,
                    microService.Package, microService.Plan, microService.UpdateStatus, CommitStatus.Invalidated);
            }
        }

        public void NotifyChanged(IComponent component)
        {
            Refresh(component.Token);
            CachingNotifications.ComponentChanged(component.MicroService, component.Folder, component.Token, component.Category);
        }

        public IComponent Select(Guid token)
        {
            var r = Get(token);

            if (r != null)
                return r;

            r = Shell.GetService<IDatabaseService>().Proxy.Development.Components.Select(token);

            if (r != null)
                Set(r.Token, r, TimeSpan.Zero);

            return r;
        }

        public IComponent Select(string category, string name)
        {
            var r = Where(f => string.Compare(f.Name, name, true) == 0
                && string.Compare(f.Category, category, true) == 0);

            if (r != null && r.Count > 0)
            {
                if (r.Count > 1)
                    throw new SysException(string.Format("{0} ({1}.{2})", SR.ErrDuplicateComponentFound, category, name));

                return r[0];
            }

            r = Shell.GetService<IDatabaseService>().Proxy.Development.Components.Query(category, name);

            if (r != null)
            {
                foreach (var i in r)
                    Set(i.Token, i, TimeSpan.Zero);

                if (r.Count > 1)
                    throw new SysException(string.Format("{0} ({1}.{2})", SR.ErrDuplicateComponentFound, category, name));

                if (r.Count > 0)
                    return r[0];
            }

            return null;
        }

        public IComponent Select(Guid microService, string category, string name)
        {
            var r = Get(f => f.MicroService == microService
                && string.Compare(f.Name, name, true) == 0
                && string.Compare(f.Category, category, true) == 0);

            if (r != null)
                return r;

            var s = DataModel.MicroServices.Select(microService);

            if (s == null)
                throw new SysException(SR.ErrMicroServiceNotFound);

            r = Shell.GetService<IDatabaseService>().Proxy.Development.Components.Select(s, category, name);

            if (r != null)
                Set(r.Token, r, TimeSpan.Zero);

            return r;
        }

        public void DropRuntimeState(Guid microService)
        {
            var state = SelectRuntimeState(microService, out Guid id);

            if (state == null)
                return;

            foreach (JObject i in state)
            {
                var prop = i.First as JProperty;

                DataModel.Blobs.Delete(prop.Value.ToString().AsGuid());
            }

            DataModel.Blobs.Delete(id);
        }

        public void SaveRuntimeState(Guid microService, Dictionary<Guid, Guid> items)
        {
            if (items.Count == 0)
                return;

            var state = new JArray();

            foreach (var i in items)
            {
                var blob = DataModel.Blobs.Select(i.Value);

                if (blob == null)
                    continue;

                var content = DataModel.BlobsContents.Select(i.Value);

                if (content == null || content.Content.Length == 0)
                    continue;

                var id = Guid.NewGuid();

                DataModel.Blobs.Upload(blob.ResourceGroup, 1001, blob.PrimaryKey, Guid.Empty, blob.Topic,
                    blob.FileName, blob.ContentType, Guid.Empty, content.Content, Storage.StoragePolicy.Singleton, id);

                state.Add(new JObject
                {
                    {i.Key.ToString(), id.ToString() }
                });
            }

            var raw = LZ4.LZ4Codec.Wrap(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state)));

            DataModel.Blobs.Upload(DataModel.ResourceGroups.Default.Token, 1002, microService.ToString(), microService,
                null, string.Format("{0}.json", microService), "application/json", Guid.Empty, raw, Storage.StoragePolicy.Singleton, Guid.NewGuid());
        }

        public JArray SelectRuntimeState(Guid microService, out Guid blobId)
        {
            blobId = Guid.Empty;

            var blobs = DataModel.Blobs.Query(DataModel.ResourceGroups.Default.Token, 1002, microService.ToString());

            if (blobs.Count == 0)
                return null;

            var content = DataModel.BlobsContents.Select(blobs[0].Token);

            if (content == null || content.Content.Length == 0)
                return null;

            blobId = blobs[0].Token;
            return JsonConvert.DeserializeObject<JArray>(Encoding.UTF8.GetString(LZ4.LZ4Codec.Unwrap(content.Content)));
        }

        public List<IComponent> QueryCategories(Guid microService, string categories)
        {
            var cats = categories.Split(',');
            var r = new List<IComponent>();

            foreach (var j in cats)
            {
                if (string.IsNullOrWhiteSpace(j))
                    continue;

                var ds = Query(microService, j.Trim());

                if (ds.Count > 0)
                    r.AddRange(ds);
            }

            return r;
        }

        public List<IComponent> Query(string resourceGroups, string categories)
        {
            var tokens = resourceGroups.Split(',');
            var cats = categories.Split(',');

            var r = new List<IComponent>();
            var microServices = new List<IMicroService>();

            foreach (var i in tokens)
            {
                var rs = DataModel.ResourceGroups.Select(i);

                if (rs == null)
                    throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, i));

                var sols = DataModel.MicroServices.Query(rs.Token);

                if (sols.Count > 0)
                    microServices.AddRange(sols);
            }

            foreach (var i in microServices)
            {
                foreach (var j in cats)
                {
                    if (string.IsNullOrWhiteSpace(j))
                        continue;

                    var ds = Query(i.Token, j.Trim());

                    if (ds.Count > 0)
                        r.AddRange(ds);
                }
            }

            return r.Where(f => f.LockVerb != LockVerb.Delete).ToList();
        }

        public List<IComponent> Query(Guid microService, bool includeDeleted)
        {
            if (includeDeleted)
                return Where(f => f.MicroService == microService);
            else
                return Where(f => f.MicroService == microService && f.LockVerb != LockVerb.Delete);
        }

        public List<IComponent> Query(Guid microService, Guid folder)
        {
            return Where(f => f.MicroService == microService && f.Folder == folder && f.LockVerb != LockVerb.Delete);
        }

        public List<IComponent> Query(Guid microService, string category)
        {
            return Where(f => f.MicroService == microService && string.Compare(f.Category, category, true) == 0 && f.LockVerb != LockVerb.Delete);
        }

        public List<IComponent> Query(Guid microService, string category, bool includeDeleted)
        {
            if (includeDeleted)
                return Where(f => f.MicroService == microService && string.Compare(f.Category, category, true) == 0 && f.LockVerb != LockVerb.Delete);
            else
                return Where(f => f.MicroService == microService && string.Compare(f.Category, category, true) == 0);
        }

        public void Insert(Guid component, Guid microService, Guid folder, string category, string name, string type, Guid runtimeConfiguration)
        {
            var s = DataModel.MicroServices.Select(microService);

            if (s == null)
                throw new SysException(SR.ErrMicroServiceNotFound);

            IFolder f = folder == Guid.Empty
                ? null
                : DataModel.Folders.Select(folder);

            if (folder != Guid.Empty && f == null)
                throw new SysException(SR.ErrFolderNotFound);

            var v = new Validator();

            v.Unique(null, name, nameof(IComponent.Name), Query(microService, category, true));

            if (!v.IsValid)
                throw new SysException(v.ErrorMessage);

            Shell.GetService<IDatabaseService>().Proxy.Development.Components.Insert(s, DateTime.UtcNow, f, category, name, component, type, runtimeConfiguration);

            Refresh(component);
            CachingNotifications.ComponentAdded(microService, folder, component, category);
        }

        public void UpdateModified(Guid microService, string category, string name)
        {
            var c = Select(microService, category, name);

            Update(c.Token, c.Name, c.Folder, c.RuntimeConfiguration);
        }

        public void Update(Guid component, string name, Guid folder)
        {
            var c = Select(component);

            if (c == null)
                throw new SysException(SR.ErrComponentNotFound);

            Update(component, name, folder, c.RuntimeConfiguration);
        }

        public void Update(Guid component, Guid runtimeConfiguration)
        {
            var c = Select(component);

            if (c == null)
                throw new SysException(SR.ErrComponentNotFound);

            Update(component, c.Name, c.Folder, runtimeConfiguration);
        }

        public void Update(Guid component, string name, Guid folder, Guid runtimeConfiguration)
        {
            var c = Select(component);

            if (c == null)
                throw new SysException(SR.ErrComponentNotFound);

            var v = new Validator();

            v.Unique(c, name, nameof(IComponent.Name), Query(c.MicroService, c.Category));

            if (!v.IsValid)
                throw new SysException(v.ErrorMessage);

            IFolder f = null;

            if (folder != Guid.Empty)
            {
                f = DataModel.Folders.Select(folder);

                if (f == null)
                    throw new SysException(SR.ErrFolderNotFound);
            }

            Shell.GetService<IDatabaseService>().Proxy.Development.Components.Update(c, DateTime.UtcNow, name, f, runtimeConfiguration);

            Refresh(component);
            CachingNotifications.ComponentChanged(c.MicroService, c.Folder, component, c.Category);
        }

        public void Delete(Guid component, Guid user, bool permanent)
        {
            var c = Select(component);

            if (c == null)
                throw new SysException(SR.ErrComponentNotFound);

            if (permanent)
            {
                Shell.GetService<IDatabaseService>().Proxy.Development.Components.Delete(c);
                DataModel.VersionControl.DeleteHistory(component);

                Remove(component);
            }
            else
            {
                var u = DataModel.Users.Select(user);

                if (u == null)
                    throw new SysException(SR.ErrUserNotFound);

                Shell.GetService<IDatabaseService>().Proxy.Development.Components.Update(c, u, LockStatus.Lock, LockVerb.Delete, DateTime.UtcNow);

                Refresh(component);
            }

            CachingNotifications.ComponentRemoved(c.MicroService, c.Folder, component, c.Category);
        }

        public string CreateComponentName(Guid microService, string prefix, string category)
        {
            var existing = Where(f => f.MicroService == microService && string.Compare(category, f.Category, true) == 0);

            return Shell.GetService<INamingService>().Create(prefix, existing.Select(f => f.Name), true);
        }

        public List<IComponent> QueryLocks(Guid microService)
        {
            if (microService == Guid.Empty)
                return Where(f => f.LockStatus == LockStatus.Lock);
            else
                return Where(f => f.LockStatus == LockStatus.Lock && f.MicroService == microService);
        }

        public List<IComponent> QueryLocks(Guid microService, Guid user)
        {
            if (microService == Guid.Empty)
                return Where(f => f.LockStatus == LockStatus.Lock && f.LockUser == user);
            else
                return Where(f => f.LockStatus == LockStatus.Lock && f.MicroService == microService && f.LockUser == user);
        }
    }
}
