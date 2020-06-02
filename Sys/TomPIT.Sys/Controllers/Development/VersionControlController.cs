using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
         var ms = body.Optional("microService", Guid.Empty);
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
      public IComponentHistory SelectNonCommited()
      {
         var body = FromBody();
         var c = body.Required<Guid>("component");

         return DataModel.VersionControl.SelectNonCommited(c);
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
      public IComponentHistory SelectCommitDetail()
      {
         var body = FromBody();
         var commit = body.Required<Guid>("commit");
         var component = body.Required<Guid>("component");

         return DataModel.VersionControl.SelectCommitDetail(commit, component);
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
         var verb = body.Required<LockVerb>("verb");

         DataModel.VersionControl.Lock(component, user, verb, blob);
      }

      [HttpPost]
      public void Undo()
      {
         var body = FromBody();
         var component = body.Required<Guid>("component");

         DataModel.VersionControl.Undo(component);
      }

      [HttpGet]
      public List<IRepository> QueryRepositories()
      {
         return DataModel.VersionControl.QueryRepositories();
      }
      [HttpGet]
      public List<IMicroServiceBinding> QueryActiveBindings()
      {
         return DataModel.VersionControl.QueryActiveBindings();
      }
      [HttpPost]
      public IMicroServiceBinding SelectBinding()
      {
         var body = FromBody();
         var service = body.Required<Guid>("service");
         var repo = body.Required<string>("repository");

         return DataModel.VersionControl.SelectBinding(service, repo);
      }
      [HttpPost]
      public List<IMicroServiceBinding> QueryBindings()
      {
         var body = FromBody();
         var service = body.Required<Guid>("service");

         return DataModel.VersionControl.QueryBindings(service);
      }
      [HttpPost]
      public void UpdateBinding()
      {
         var body = FromBody();
         var service = body.Required<Guid>("service");
         var repository = body.Required<string>("repository");
         var commit = body.Required<long>("commit");
         var date = body.Required<DateTime>("date");
         var active = body.Required<bool>("active");

         DataModel.VersionControl.UpdateBinding(service, repository, commit, date, active);
      }
      [HttpPost]
      public void DeleteBinding()
      {
         var body = FromBody();
         var service = body.Required<Guid>("service");
         var repository = body.Required<string>("repository");

         DataModel.VersionControl.DeleteBinding(service, repository);
      }
      [HttpPost]
      public void InsertRepository()
      {
         var body = FromBody();
         var name = body.Required<string>("name");
         var url = body.Required<string>("url");
         var userName = body.Required<string>("userName");
         var password = body.Required<string>("password");

         DataModel.VersionControl.InsertRepository(name, url, userName, password);
      }
      [HttpPost]
      public void UpdateRepository()
      {
         var body = FromBody();

         var existingName = body.Required<string>("existingName");
         var name = body.Required<string>("name");
         var url = body.Required<string>("url");
         var userName = body.Required<string>("userName");
         var password = body.Required<string>("password");

         DataModel.VersionControl.UpdateRepository(existingName, name, url, userName, password);
      }
      [HttpPost]
      public void DeleteRepository()
      {
         var body = FromBody();

         var existingName = body.Required<string>("existingName");
         var name = body.Required<string>("name");
         var url = body.Required<string>("url");
         var userName = body.Required<string>("userName");
         var password = body.Required<string>("password");

         DataModel.VersionControl.UpdateRepository(existingName, name, url, userName, password);
      }
      [HttpPost]
      public IRepository SelectRepository()
      {
         var body = FromBody();

         var name = body.Required<string>("name");

         return DataModel.VersionControl.SelectRepository(name);
      }
   }
}
