using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Development;
using TomPIT.Security;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
   internal class VersionControlHandler : IVersionControlHandler
   {
      public void DeleteCommit(ICommit commit)
      {
         var w = new Writer("tompit.version_control_commit_del");

         w.CreateParameter("@id", commit.GetId());

         w.Execute();
      }

      public void InsertCommit(Guid token, IMicroService service, IUser user, DateTime created, string comment, List<IComponent> components)
      {
         var w = new Writer("tompit.version_control_commit_ins");

         w.CreateParameter("@token", token);
         w.CreateParameter("@service", service.Token);
         w.CreateParameter("@user", user.GetId());
         w.CreateParameter("@created", created);
         w.CreateParameter("@comment", comment);

         var a = new JArray();

         foreach (var i in components)
         {
            a.Add(new JObject
                {
                    {"token", i.Token }
                });
         }

         w.CreateParameter("@components", a);

         w.Execute();
      }

      public void InsertComponentHistory(IUser user, IComponent component, string name, LockVerb verb, DateTime created, Guid blob)
      {
         var w = new Writer("tompit.component_history_ins");

         w.CreateParameter("@user", user.GetId());
         w.CreateParameter("@component", component.Token);
         w.CreateParameter("@name", name);
         w.CreateParameter("@created", created);
         w.CreateParameter("@configuration", blob);
         w.CreateParameter("@verb", verb);

         w.Execute();
      }

      public void UndoComponentHistory(IComponent component)
      {
         var w = new Writer("tompit.component_history_undo");

         w.CreateParameter("@component", component.Token);

         w.Execute();
      }

      public List<IComponent> QueryCommitComponents(ICommit commit)
      {
         var r = new Reader<Component>("tompit.component_commit_que");

         r.CreateParameter("@commit", commit.GetId());

         return r.Execute().ToList<IComponent>();
      }

      public List<ICommit> QueryCommits(IMicroService service)
      {
         var r = new Reader<Commit>("tompit.version_control_commit_que");

         r.CreateParameter("@service", service.Token);

         return r.Execute().ToList<ICommit>();
      }

      public List<ICommit> QueryCommits(IMicroService service, IComponent component)
      {
         var r = new Reader<Commit>("tompit.version_control_commit_que");

         r.CreateParameter("@service", service.Token);
         r.CreateParameter("@component", component.Token);

         return r.Execute().ToList<ICommit>();
      }

      public List<ICommit> QueryCommits(IMicroService service, IUser user)
      {
         var r = new Reader<Commit>("tompit.version_control_commit_que");

         r.CreateParameter("@service", service.Token);
         r.CreateParameter("@user", user.GetId(), true);

         return r.Execute().ToList<ICommit>();
      }

      public ICommit SelectCommit(Guid token)
      {
         var r = new Reader<Commit>("tompit.version_control_commit_sel");

         r.CreateParameter("@token", token);

         return r.ExecuteSingleRow();
      }

      public IComponentHistory SelectNonCommited(Guid component)
      {
         var r = new Reader<ComponentHistory>("tompit.component_history_open_sel");

         r.CreateParameter("@component", component);

         return r.ExecuteSingleRow();
      }

      public List<IComponentHistory> QueryHistory(IComponent component)
      {
         var r = new Reader<ComponentHistory>("tompit.component_history_que");

         r.CreateParameter("@component", component.Token);

         return r.Execute().ToList<IComponentHistory>();
      }

      public List<IComponentHistory> QueryCommitDetails(ICommit commit)
      {
         var r = new Reader<ComponentHistory>("tompit.component_history_que");

         r.CreateParameter("@commit", commit.GetId());

         return r.Execute().ToList<IComponentHistory>();
      }

      public IComponentHistory SelectCommitDetail(ICommit commit, Guid component)
      {
         var r = new Reader<ComponentHistory>("tompit.component_history_sel");

         r.CreateParameter("@commit", commit.GetId());
         r.CreateParameter("@component", component);

         return r.ExecuteSingleRow();
      }

      public void DeleteHistory(IComponentHistory history)
      {
         var w = new Writer("tompit.component_history_del");

         w.CreateParameter("@id", history.GetId());

         w.Execute();
      }
   }
}
