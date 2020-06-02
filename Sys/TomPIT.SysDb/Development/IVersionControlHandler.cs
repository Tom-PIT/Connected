using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Security;

namespace TomPIT.SysDb.Development
{
   public interface IVersionControlHandler
   {
      void InsertCommit(Guid token, IMicroService service, IUser user, DateTime created, string comment, List<IComponent> components);

      ICommit SelectCommit(Guid token);

      List<ICommit> QueryCommits(IMicroService service);
      List<ICommit> QueryCommits(IMicroService service, IComponent component);
      List<ICommit> QueryCommits(IMicroService service, IUser user);

      List<IComponent> QueryCommitComponents(ICommit commit);
      void DeleteCommit(ICommit commit);
      void DeleteHistory(IComponentHistory history);

      void InsertComponentHistory(IUser user, IComponent component, string name, LockVerb verb, DateTime created, Guid blob);
      void UndoComponentHistory(IComponent component);
      List<IComponentHistory> QueryHistory(IComponent component);
      List<IComponentHistory> QueryCommitDetails(ICommit commit);
      IComponentHistory SelectCommitDetail(ICommit commit, Guid component);
      IComponentHistory SelectNonCommited(Guid component);

      List<IRepository> QueryRepositories();
      List<IMicroServiceBinding> QueryActiveBindings();
      IMicroServiceBinding SelectBinding(IMicroService service, IRepository repository);
      List<IMicroServiceBinding> QueryBindings(IMicroService service);
      void UpdateBinding(IMicroService service, IRepository repository, long commit, DateTime date, bool active);
      void DeleteBinding(IMicroService service, IRepository repository);
      void InsertRepository(string name, string url, string userName, byte[] password);
      void UpdateRepository(IRepository repository, string name, string url, string userName, byte[] password);
      void DeleteRepository(IRepository repository);
      IRepository SelectRepository(string name);
   }
}
